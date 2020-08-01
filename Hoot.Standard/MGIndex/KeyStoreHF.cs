﻿using RaptorDB.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RaptorDB
{
    // high frequency key value store
    public class KeyStoreHF : IKeyStoreHF
    {
        internal class AllocationBlock
        {
            public string key;
            public byte keylen;
            public int datalength;
            public bool isCompressed;
            public bool isBinaryJSON;
            public bool deleteKey;
            public List<int> Blocks = new List<int>();
            public int blocknumber;
        }

        MGIndex<string> _keys;
        StorageFileHF _datastore;
        object _lock = new object();
        ushort _BlockSize = 2048;
        private const int _KILOBYTE = 1024;
        ILog _log = LogManager.GetLogger(typeof(KeyStoreHF));

        private object _dfile = new object();

        byte[] _blockheader = new byte[]{
            0,0,0,0,    // 0  block # (used for validate block reads and rebuild)
            0,0,0,0,    // 4  next block # 
            0,          // 8  flags bits 0:iscompressed  1:isbinary  2:deletekey
            0,0,0,0,    // 9  data length (compute alloc blocks needed)
            0,          // 13 key length 
            0,          // 14 key type 0=guid 1=string
        };

        private bool _isDirty = false;
        private string _dirtyFilename = "temp.$";
        private String _filePath = String.Empty;

        public KeyStoreHF(string folder)
        {
            _filePath = folder;

            if (!Directory.Exists(_filePath))
                Directory.CreateDirectory(_filePath);
            
            if (File.Exists(Path.Combine(_filePath, _dirtyFilename)))
            {
                _log.Error("Last shutdown failed, rebuilding data files...");
                RebuildDataFiles();
            }
            _datastore = new StorageFileHF(Path.Combine(_filePath, "data.mghf"), Global.HighFrequencyKVDiskBlockSize);
            _keys = new MGIndex<string>(Path.Combine(_filePath, "keys.idx"), 255, /*Global.PageItemCount,*/ false);
            //_datastore.Initialize();
            _BlockSize = _datastore.GetBlockSize();
        }

        // mgindex special storage for strings ctor -> no idx file
        //    use SaveData() GetData()
        public KeyStoreHF(string folder, string filename)
        {
            _filePath = folder;

            if (!Directory.Exists(_filePath))
                Directory.CreateDirectory(_filePath);

            _datastore = new StorageFileHF(Path.Combine(_filePath, filename), Global.HighFrequencyKVDiskBlockSize);
            //_datastore.Initialize();
            _BlockSize = _datastore.GetBlockSize();
        }

        public int CountHF()
        {
            if (_keys != null)
                return _keys.Count();
            else
                return 0;
        }

        public object GetObjectHF(string key)
        {
            lock (_lock)
            {
                int alloc;
                if (_keys.Get(key, out alloc))
                {
                    AllocationBlock ab = FillAllocationBlock(alloc);
                    if (ab.deleteKey == false)
                    {
                        byte[] data = readblockdata(ab);

                        return fastBinaryJSON.BJSON.ToObject(data);
                    }
                }
            }

            return null;
        }

        public bool SetObjectHF(string key, object obj)
        {
            byte[] k = Helper.GetBytes(key);
            if (k.Length > 255)
            {
                _log.Error("Key length > 255 : " + key);
                throw new Exception("Key must be less than 255 characters");
                //return false;
            }
            lock (_lock)
            {
                if (_isDirty == false)
                    WriteDirtyFile();

                AllocationBlock ab = null;
                int firstblock = 0;
                if (_keys.Get(key, out firstblock))// key exists already
                    ab = FillAllocationBlock(firstblock);

                SaveNew(key, k, obj);
                if (ab != null)
                {
                    // free old blocks
                    ab.Blocks.Add(ab.blocknumber);
                    _datastore.FreeBlocks(ab.Blocks);
                }
                return true;
            }
        }

        public bool DeleteKeyHF(string key)
        {
            lock (_lock)
            {
                int alloc;
                if (_keys.Get(key, out alloc))
                {
                    if (_isDirty == false)
                        WriteDirtyFile();

                    byte[] keybytes = Helper.GetBytes(key);
                    AllocationBlock ab = FillAllocationBlock(alloc);

                    ab.keylen = (byte)keybytes.Length;

                    _keys.RemoveKey(key);// remove key from index

                    // write ab
                    ab.deleteKey = true;
                    ab.datalength = 0;

                    byte[] header = CreateAllocHeader(ab, keybytes);

                    _datastore.SeekBlock(ab.blocknumber);
                    _datastore.WriteBlockBytes(header, 0, header.Length);

                    // free old data blocks
                    _datastore.FreeBlocks(ab.Blocks);

                    return true;
                }
            }
            return false;
        }

        public void CompactStorageHF(String folder)
        {
            lock (_lock)
            {
                try
                {
                    String _path = Path.Combine(folder, "temp");

                    _log.Debug("Compacting storage file ...");

                    if (Directory.Exists(_path))
                        Directory.Delete(_path, true);

                    KeyStoreHF newfile = new KeyStoreHF(Path.Combine(_path, "temp"));

                    string[] keys = _keys.GetKeys().Cast<string>().ToArray();

                    _log.Debug("Number of keys : " + keys.Length);
 
                    foreach (var k in keys)
                    {
                        newfile.SetObjectHF(k, GetObjectHF(k));
                    }
                    newfile.Shutdown();
                    _log.Debug("Compact done.");
                    //
                    // shutdown and move files and restart here
                    //
                    _path = Path.Combine(folder, "old");

                    if (Directory.Exists(_path))
                        Directory.Delete(_path, true);

                    Directory.CreateDirectory(_path);

                    _datastore.Shutdown();
                    _keys.Shutdown();
                    
                    _log.Debug("Moving files...");
                    
                    foreach (var f in Directory.GetFiles(folder, "*.*"))
                        File.Move(f, Path.Combine(folder,"old", Path.GetFileName(f)));

                    foreach (var f in Directory.GetFiles(Path.Combine(folder,"temp"), "*.*"))
                        File.Move(f, Path.Combine(folder, Path.GetFileName(f)));

                    Directory.Delete(Path.Combine(folder, "temp"), true);

                    //Directory.Delete(_Path + "old", true); // FEATURE : delete or keep?

                    _log.Debug("Re-opening storage file");

                    _datastore = new StorageFileHF(Path.Combine(folder,"data.mghf"), Global.HighFrequencyKVDiskBlockSize);
                    _keys = new MGIndex<string>(Path.Combine(folder,"keys.idx"), 255, false);

                    _BlockSize = _datastore.GetBlockSize();
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                }
            }
        }

        public string[] GetKeysHF()
        {
            lock (_lock)
                return _keys.GetKeys().Cast<string>().ToArray(); // FEATURE : ugly and dirty !?
        }

        public bool ContainsHF(string key)
        {
            lock (_lock)
            {
                int i = 0;
                return _keys.Get(key, out i);
            }
        }

        public void Shutdown()
        {
            _datastore.Shutdown();
            if (_keys != null)
            {
                _keys.Shutdown();

                if (File.Exists(Path.Combine(_filePath,_dirtyFilename)))
                    File.Delete(Path.Combine(_filePath, _dirtyFilename));
            }
        }

        internal void FreeMemory()
        {
            if (_keys != null)
                _keys.FreeMemory();
        }

        #region Private Methods

        private byte[] readblockdata(AllocationBlock ab)
        {
            byte[] data = new byte[ab.datalength];
            long offset = 0;
            int len = ab.datalength;
            int dbsize = _BlockSize - _blockheader.Length - ab.keylen;
            ab.Blocks.ForEach(x =>
            {
                byte[] b = _datastore.ReadBlock(x);
                int c = len;
                if (c > dbsize) c = dbsize;
                Buffer.BlockCopy(b, _blockheader.Length + ab.keylen, data, (int)offset, c);
                offset += c;
                len -= c;
            });
            if (ab.isCompressed)
                data = MiniLZO.Decompress(data);
            return data;
        }

        private void WriteDirtyFile()
        {
            lock (_dfile)
            {
                _isDirty = true;
                if (File.Exists(Path.Combine(_filePath,_dirtyFilename)) == false)
                    File.WriteAllText(Path.Combine(_filePath, _dirtyFilename), "dirty");
            }
        }

        private void SaveNew(string key, byte[] keybytes, object obj)
        {
            byte[] data;
            AllocationBlock ab = new AllocationBlock();
            ab.key = key;
            ab.keylen = (byte)keybytes.Length;

            data = fastBinaryJSON.BJSON.ToBJSON(obj);
            ab.isBinaryJSON = true;

            if (data.Length > (int)Global.CompressDocumentOverKiloBytes * _KILOBYTE)
            {
                ab.isCompressed = true;
                data = MiniLZO.Compress(data);
            }
            ab.datalength = data.Length;

            int firstblock = internalSave(keybytes, data, ab);

            // save keys
            _keys.Set(key, firstblock);
        }

        private int internalSave(byte[] keybytes, byte[] data, AllocationBlock ab)
        {
            int firstblock = _datastore.GetFreeBlockNumber();
            int blocknum = firstblock;
            byte[] header = CreateAllocHeader(ab, keybytes);
            int dblocksize = _BlockSize - header.Length;
            int offset = 0;
            // compute data block count
            int datablockcount = (data.Length / dblocksize) + 1;
            // save data blocks
            int counter = 0;
            int len = data.Length;
            while (datablockcount > 0)
            {
                datablockcount--;
                int next = 0;
                if (datablockcount > 0)
                    next = _datastore.GetFreeBlockNumber();

                Buffer.BlockCopy(Helper.GetBytes(counter, false), 0, header, 0, 4);    // set block number
                Buffer.BlockCopy(Helper.GetBytes(next, false), 0, header, 4, 4); // set next pointer

                _datastore.SeekBlock(blocknum);
                _datastore.WriteBlockBytes(header, 0, header.Length);
                int c = len;
                if (c > dblocksize)
                    c = dblocksize;
                _datastore.WriteBlockBytes(data, offset, c);

                if (next > 0)
                    blocknum = next;
                offset += c;
                len -= c;
                counter++;
            }
            return firstblock;
        }

        private byte[] CreateAllocHeader(AllocationBlock ab, byte[] keybytes)
        {
            byte[] alloc = new byte[_blockheader.Length + keybytes.Length];

            if (ab.isCompressed)
                alloc[8] = 1;
            if (ab.isBinaryJSON)
                alloc[8] += 2;
            if (ab.deleteKey)
                alloc[8] += 4;

            Buffer.BlockCopy(Helper.GetBytes(ab.datalength, false), 0, alloc, 9, 4);
            alloc[13] = ab.keylen;
            alloc[14] = 1; // string keys for now
            Buffer.BlockCopy(keybytes, 0, alloc, _blockheader.Length, ab.keylen);

            return alloc;
        }

        private AllocationBlock FillAllocationBlock(int blocknumber)
        {
            AllocationBlock ab = new AllocationBlock();

            ab.blocknumber = blocknumber;
            ab.Blocks.Add(blocknumber);

            byte[] b = _datastore.ReadBlockBytes(blocknumber, _blockheader.Length + 255);

            int blocknumexpected = 0;

            int next = ParseBlockHeader(ab, b, blocknumexpected);

            blocknumexpected++;

            while (next > 0)
            {
                ab.Blocks.Add(next);
                b = _datastore.ReadBlockBytes(next, _blockheader.Length + ab.keylen);
                next = ParseBlockHeader(ab, b, blocknumexpected);
                blocknumexpected++;
            }

            return ab;
        }

        private int ParseBlockHeader(AllocationBlock ab, byte[] b, int blocknumberexpected)
        {
            int bnum = Helper.ToInt32(b, 0);
            if (bnum != blocknumberexpected)
            {
                _log.Error("Block numbers does not match, looking for : " + blocknumberexpected);
                //throw new Exception("Block numbers does not match, looking for : " + blocknumberexpected);
                return -1;
            }
            if (b[14] != 1)
            {
                _log.Error("Expecting string keys only, got : " + b[14]);
                //throw new Exception("Expecting string keys only, got : " + b[11]);
                return -1;
            }

            int next = Helper.ToInt32(b, 4);

            if (ab.keylen == 0)
            {
                byte flags = b[8];

                if ((flags & 0x01) > 0)
                    ab.isCompressed = true;
                if ((flags & 0x02) > 0)
                    ab.isBinaryJSON = true;
                if ((flags & 0x04) > 0)
                    ab.deleteKey = true;

                ab.datalength = Helper.ToInt32(b, 9);
                byte keylen = b[13];
                ab.keylen = keylen;
                ab.key = Helper.GetString(b, _blockheader.Length, keylen);
            }
            return next;
        }

        private void RebuildDataFiles()
        {
            MGIndex<string> keys = null;
            try
            {
                // remove old free list
                if (File.Exists(Path.Combine(_filePath,"data.bmp")))
                    File.Delete(Path.Combine(_filePath,"data.bmp"));

                _datastore = new StorageFileHF(Path.Combine(_filePath,"data.mghf"), Global.HighFrequencyKVDiskBlockSize);
                _BlockSize = _datastore.GetBlockSize();
                if (File.Exists(Path.Combine(_filePath,"keys.idx")))
                {
                    _log.Debug("removing old keys index");
                    foreach (var f in Directory.GetFiles(_filePath, "keys.*"))
                        File.Delete(f);
                }

                keys = new MGIndex<string>(Path.Combine(_filePath, "keys.idx"), 255, /*Global.PageItemCount,*/ false);

                MGRB visited = new MGRB();

                int c = _datastore.NumberofBlocks();

                for (int i = 1; i < c; i++) // go through blocks skip first
                {
                    if (visited.Get(i))
                        continue;
                    byte[] b = _datastore.ReadBlockBytes(i, _blockheader.Length + 255);
                    int bnum = Helper.ToInt32(b, 0);
                    if (bnum > 0) // check if a start block
                    {
                        visited.Set(i, true);
                        _datastore.FreeBlock(i); // mark as free
                        continue;
                    }

                    AllocationBlock ab = new AllocationBlock();
                    // start block found
                    int blocknumexpected = 0;

                    int next = ParseBlockHeader(ab, b, blocknumexpected);
                    int last = 0;
                    bool freelast = false;
                    AllocationBlock old = null;

                    if (ab.key == null)
                        continue;

                    if (keys.Get(ab.key, out last))
                    {
                        old = this.FillAllocationBlock(last);
                        freelast = true;
                    }
                    blocknumexpected++;
                    bool failed = false;
                    if (ab.deleteKey == false)
                    {
                        while (next > 0) // read the blocks
                        {
                            ab.Blocks.Add(next);
                            b = _datastore.ReadBlockBytes(next, _blockheader.Length + ab.keylen);
                            next = ParseBlockHeader(ab, b, blocknumexpected);
                            if (next == -1) // non matching block
                            {
                                failed = true;
                                break;
                            }
                            blocknumexpected++;
                        }
                    }
                    else
                    {
                        failed = true;
                        keys.RemoveKey(ab.key);
                    }
                    // new data ok
                    if (failed == false)
                    {
                        keys.Set(ab.key, i);// valid block found
                        if (freelast && old != null)// free the old blocks
                            _datastore.FreeBlocks(old.Blocks);
                    }

                    visited.Set(i, true);
                }

                // all ok delete temp.$ file
                if (File.Exists(Path.Combine(_filePath,_dirtyFilename)))
                    File.Delete(Path.Combine(_filePath,_dirtyFilename));
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
            finally
            {
                _log.Debug("Shutting down files and index");
                _datastore.Shutdown();
                keys.SaveIndex();
                keys.Shutdown();
            }
        }
        #endregion

        internal void FreeBlocks(List<int> list)
        {
            lock (_lock)
                _datastore.FreeBlocks(list);
        }


        // for .string files
        internal int SaveData(string key, byte[] data)
        {
            lock (_lock)
            {
                byte[] kb = Helper.GetBytes(key);
                AllocationBlock ab = new AllocationBlock();
                ab.key = key;
                ab.keylen = (byte)kb.Length;
                ab.isCompressed = false;
                ab.isBinaryJSON = true;
                ab.datalength = data.Length;

                return internalSave(kb, data, ab);
            }
        }

        // for .string files
        internal byte[] GetData(int blocknumber, out List<int> usedblocks)
        {
            lock (_lock)
            {
                AllocationBlock ab = FillAllocationBlock(blocknumber);
                usedblocks = ab.Blocks;
                byte[] data = readblockdata(ab);

                return data;
            }
        }

        public int Increment(string key, int amount)
        {
            byte[] k = Helper.GetBytes(key);
            if (k.Length > 255)
            {
                _log.Error("Key length > 255 : " + key);
                throw new Exception("Key must be less than 255 characters");
                //return false;
            }
            lock (_lock)
            {
                if (_isDirty == false)
                    WriteDirtyFile();

                AllocationBlock ab = null;
                int firstblock = 0;
                if (_keys.Get(key, out firstblock))// key exists already
                    ab = FillAllocationBlock(firstblock);

                object obj = amount;
                if (ab.deleteKey == false)
                {
                    byte[] data = readblockdata(ab);

                    obj = fastBinaryJSON.BJSON.ToObject(data);

                    // add here
                    if (obj is int)
                        obj = ((int)obj) + amount;
                    else if (obj is long)
                        obj = ((long)obj) + amount;
                    else if (obj is decimal)
                        obj = ((decimal)obj) + amount;
                    else
                        return (int)obj;
                }

                SaveNew(key, k, obj);
                if (ab != null)
                {
                    // free old blocks
                    ab.Blocks.Add(ab.blocknumber);
                    _datastore.FreeBlocks(ab.Blocks);
                }
                return (int)obj;
            }
        }

        public int Decrement(string key, int amount)
        {
            return (int)Increment(key, -amount);
        }

        public decimal Increment(string key, decimal amount)
        {
            byte[] k = Helper.GetBytes(key);
            if (k.Length > 255)
            {
                _log.Error("Key length > 255 : " + key);
                throw new Exception("Key must be less than 255 characters");
                //return false;
            }
            lock (_lock)
            {
                if (_isDirty == false)
                    WriteDirtyFile();

                AllocationBlock ab = null;
                int firstblock = 0;
                if (_keys.Get(key, out firstblock))// key exists already
                    ab = FillAllocationBlock(firstblock);

                object obj = amount;
                if (ab.deleteKey == false)
                {
                    byte[] data = readblockdata(ab);

                    obj = fastBinaryJSON.BJSON.ToObject(data);

                    // add here
                    if (obj is decimal)
                        obj = ((decimal)obj) + amount;
                    else
                        return (decimal)obj;
                }

                SaveNew(key, k, obj);
                if (ab != null)
                {
                    // free old blocks
                    ab.Blocks.Add(ab.blocknumber);
                    _datastore.FreeBlocks(ab.Blocks);
                }
                return (decimal)obj;
            }
        }

        public decimal Decrement(string key, decimal amount)
        {
            return Increment(key, -amount);
        }
    }
}
