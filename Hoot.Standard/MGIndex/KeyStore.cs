﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using RaptorDB.Common;

namespace RaptorDB
{
    #region [   KeyStoreString   ]
    internal class KeyStoreString : IDisposable
    {
        private bool _caseSensitive = false;
        private KeyStore<int> _db;

        public KeyStoreString(String filename, bool caseSensitve)
        {
            _db = KeyStore<int>.Open(filename, true);
            _caseSensitive = caseSensitve;
        }

        public void Set(string key, string val)
        {
            Set(key, fastJSON.Reflection.UnicodeGetBytes(val));
        }

        public void Set(string key, byte[] val)
        {
            string str = (_caseSensitive ? key : key.ToLower());
            byte[] bkey = fastJSON.Reflection.UnicodeGetBytes(str);
            int hc = (int)Helper.MurMur.Hash(bkey);
            MemoryStream ms = new MemoryStream();
            ms.Write(Helper.GetBytes(bkey.Length, false), 0, 4);
            ms.Write(bkey, 0, bkey.Length);
            ms.Write(val, 0, val.Length);

            _db.SetBytes(hc, ms.ToArray());
        }

        public bool Get(string key, out string val)
        {
            val = null;
            byte[] bval;
            bool b = Get(key, out bval);
            if (b)
            {
                val = fastJSON.Reflection.UnicodeGetString(bval);
            }
            return b;
        }

        public bool Get(string key, out byte[] val)
        {
            string str = (_caseSensitive ? key : key.ToLower());
            val = null;
            byte[] bkey = fastJSON.Reflection.UnicodeGetBytes(str);
            int hc = (int)Helper.MurMur.Hash(bkey);

            if (_db.GetBytes(hc, out val))
            {
                // unpack data
                byte[] g = null;
                if (UnpackData(val, out val, out g))
                {
                    if (Helper.CompareMemCmp(bkey, g) != 0)
                    {
                        // if data not equal check duplicates (hash conflict)
                        List<int> ints = new List<int>(_db.GetDuplicates(hc));
                        ints.Reverse();
                        foreach (int i in ints)
                        {
                            byte[] bb = _db.FetchRecordBytes(i);
                            if (UnpackData(bb, out val, out g))
                            {
                                if (Helper.CompareMemCmp(bkey, g) == 0)
                                    return true;
                            }
                        }
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }

        public int Count()
        {
            return (int)_db.Count();
        }

        public int RecordCount()
        {
            return (int)_db.RecordCount();
        }

        public void SaveIndex()
        {
            _db.SaveIndex();
        }

        public void Shutdown()
        {
            _db.Shutdown();
        }

        public void Dispose()
        {
            _db.Shutdown();
        }

        private bool UnpackData(byte[] buffer, out byte[] val, out byte[] key)
        {
            int len = Helper.ToInt32(buffer, 0, false);
            key = new byte[len];
            Buffer.BlockCopy(buffer, 4, key, 0, len);
            val = new byte[buffer.Length - 4 - len];
            Buffer.BlockCopy(buffer, 4 + len, val, 0, buffer.Length - 4 - len);

            return true;
        }

        public string ReadData(int recnumber)
        {
            byte[] val;
            byte[] key;
            byte[] b = _db.FetchRecordBytes(recnumber);
            if (UnpackData(b, out val, out key))
            {
                return fastJSON.Reflection.UnicodeGetString(val);
            }
            return "";
        }

        internal void FreeMemory()
        {
            _db.FreeMemory();
        }
    }
    #endregion
     
    internal class KeyStore<T> : IDisposable, IDocStorage<T> where T : IComparable<T>
    {
        private ILog log = LogManager.GetLogger(typeof(KeyStore<T>));

        //private string _Path = "";
        //private string _FileName = "";
        private byte _MaxKeySize;
        private StorageFile<T> _archive;
        private MGIndex<T> _index;
        private string _datExtension = ".mgdat";
        private string _idxExtension = ".mgidx";
        private IGetBytes<T> _T = null;
        private System.Timers.Timer _savetimer;
        private BoolIndex _deleted;
        private object _savelock = new object();
        private object _shutdownlock = new object();

        public KeyStore(String filename, bool AllowDuplicateKeys)
            : this(filename, Global.DefaultStringKeySize, AllowDuplicateKeys)
        {

        }

        public KeyStore(String filename, byte MaxKeySize, bool AllowDuplicateKeys)
        {
            Initialize(filename, MaxKeySize, AllowDuplicateKeys);
        }

        public static KeyStore<T> Open(String filename, bool AllowDuplicateKeys)
        {
            return new KeyStore<T>(filename, AllowDuplicateKeys);
        }

        public static KeyStore<T> Open(String filename, byte MaxKeySize, bool AllowDuplicateKeys)
        {
            return new KeyStore<T>(filename, MaxKeySize, AllowDuplicateKeys);
        }

        /// <summary>
        /// Save the index file
        /// </summary>
        public void SaveIndex()
        {
            if (_index != null)
            {
                lock (_savelock)
                {
                    log.Debug("saving to disk");
                    _index.SaveIndex();
                    _deleted.SaveIndex();
                    log.Debug("index saved");
                }
            }
        }

        public IEnumerable<int> GetDuplicates(T key)
        {
            // get duplicates from index
            return _index.GetDuplicates(key);
        }

        public byte[] FetchRecordBytes(int record)
        {
            return _archive.ReadBytes(record);
        }

        public long Count()
        {
            int c = _archive.Count();
            return c - _deleted.GetBits().CountOnes() * 2;
        }

        public bool Get(T key, out string val)
        {
            byte[] b = null;
            val = "";
            bool ret = GetBytes(key, out b);
            if (ret)
            {
                if (b != null)
                    val = fastJSON.Reflection.UnicodeGetString(b);
                else
                    val = "";
            }
            return ret;
        }

        public bool GetObject(T key, out object val)
        {
            int off;
            val = null;
            if (_index.Get(key, out off))
            {
                val = _archive.ReadObject(off);
                return true;
            }
            return false;
        }

        public bool GetBytes(T key, out byte[] val)
        {
            int off;
            val = null;
            // search index
            if (_index.Get(key, out off))
            {
                val = _archive.ReadBytes(off);
                return true;
            }
            return false;
        }

        public int SetString(T key, string data)
        {
            return SetBytes(key, fastJSON.Reflection.UnicodeGetBytes(data));
        }
        /// <summary>
        /// Save Record to Storage and save index
        /// </summary>
        /// <param name="key"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        public int SetObject(T key, object doc)
        {
            int recno = -1;

            recno = (int) _archive.WriteObject(key, doc);
            _index.Set(key, recno);
            return recno;
        }
        /// <summary>
        /// Write Record to Storage and Save Index
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public int SetBytes(T key, byte[] data)
        {
            int recno = -1;

            recno = (int)_archive.WriteData(key, data);
            _index.Set(key, recno);
            return recno;
        }
        /// <summary>
        /// Shut down the KeyStorage Indexer
        /// </summary>
        public void Shutdown()
        {
            lock (_shutdownlock)
            {
                if (_index != null)
                {
                    log.Debug("Shutting down");

                    _savetimer.Enabled = false;
                    SaveIndex();
                    SaveLastRecord();

                    if (_deleted != null)
                        _deleted.Shutdown();

                    if (_index != null)
                        _index.Shutdown();

                    if (_archive != null)
                        _archive.Shutdown();

                    _index = null;
                    _archive = null;
                    _deleted = null;
                }
            }
        }

        public void Dispose()
        {
            Shutdown();
        }

        #region Private Methods

        /// <summary>
        /// save the last record number in the index file
        /// </summary>
        private void SaveLastRecord()
        {
            _index.SaveLastRecordNumber(_archive.Count());
        }
        /// <summary>
        /// Initialize the Indexer
        /// </summary>
        /// <param name="config"></param>
        /// <param name="maxkeysize"></param>
        /// <param name="AllowDuplicateKeys"></param>
        private void Initialize(String filename, byte maxkeysize, bool AllowDuplicateKeys)
        {
            _MaxKeySize = RDBDataType<T>.GetByteSize(maxkeysize);
            _T = RDBDataType<T>.ByteHandler();

            string db = Path.ChangeExtension(filename, _datExtension);
            string idx = Path.ChangeExtension(filename, _idxExtension);

            //LogManager.Configure(_Path + Path.DirectorySeparatorChar + _FileName + ".txt", 500, false);

            _index = new MGIndex<T>(idx, _MaxKeySize, /*Global.PageItemCount,*/ AllowDuplicateKeys);

            if (Global.SaveAsBinaryJSON)
                _archive = new StorageFile<T>(db, SF_FORMAT.BSON, false);
            else
                _archive = new StorageFile<T>(db, SF_FORMAT.JSON, false);

            _deleted = new BoolIndex(Path.Combine(Path.GetDirectoryName(filename),"_deleted.idx"));

            log.Debug("Current Count = " + RecordCount().ToString("#,0"));

            CheckIndexState();

            log.Debug("Starting save timer");
            _savetimer = new System.Timers.Timer();
            _savetimer.Elapsed += new System.Timers.ElapsedEventHandler(_savetimer_Elapsed);
            _savetimer.Interval = Global.SaveIndexToDiskTimerSeconds * 1000;
            _savetimer.AutoReset = true;
            _savetimer.Start();

        }

        private void CheckIndexState()
        {
            log.Debug("Checking Index state...");
            int last = _index.GetLastIndexedRecordNumber();
            int count = _archive.Count();
            if (last < count)
            {
                log.Debug("Rebuilding index...");
                log.Debug("   last index count = " + last);
                log.Debug("   data items count = " + count);
                // check last index record and archive record
                //       rebuild index if needed
                for (int i = last; i < count; i++)
                {
                    bool deleted = false;
                    T key = _archive.GetKey(i, out deleted);
                    if (deleted == false)
                        _index.Set(key, i);
                    else
                        _index.RemoveKey(key);

                    if (i % 100000 == 0)
                        log.Debug("100,000 items re-indexed");
                }
                log.Debug("Rebuild index done.");
            }
        }

        void _savetimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            SaveIndex();
        }

        #endregion

        public int RecordCount()
        {
            return _archive.Count();
        }

        public int[] GetHistory(T key)
        {
            List<int> a = new List<int>();
            foreach (int i in GetDuplicates(key))
            {
                a.Add(i);
            }
            return a.ToArray();
        }

        internal byte[] FetchRecordBytes(int record, out bool isdeleted)
        {
            StorageItem<T> meta;
            byte[] b = _archive.ReadBytes(record, out meta);
            isdeleted = meta.isDeleted;
            return b;
        }
        /// <summary>
        /// write a delete record
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal bool Delete(T id)
        {
            int rec = (int)_archive.Delete(id);

            _deleted.Set(true, rec);
            return _index.RemoveKey(id);
        }
        /// <summary>
        /// write a delete record for replicated object
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal bool DeleteReplicated(T id)
        {
            int rec = (int)_archive.DeleteReplicated(id);
            _deleted.Set(true, rec);
            return _index.RemoveKey(id);
        }

        internal int CopyTo(StorageFile<T> storagefile, long startrecord)
        {
            return _archive.CopyTo(storagefile, startrecord);
        }

        public byte[] GetBytes(int rowid, out StorageItem<T> meta)
        {
            return _archive.ReadBytes(rowid, out meta);
        }

        internal void FreeMemory()
        {
            _index.FreeMemory();
        }

        public object GetObject(int rowid, out StorageItem<T> meta)
        {
            return _archive.ReadObject(rowid, out meta);
        }

        public StorageItem<T> GetMeta(int rowid)
        {
            return _archive.ReadMeta(rowid);
        }
        /// <summary>
        /// Save to Record Storage and Index
        /// </summary>
        /// <param name="key"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        internal int SetReplicationObject(T key, object doc)
        {
            int recno = -1;

            recno = (int) _archive.WriteReplicationObject(key, doc);
            _index.Set(key, recno);
            return recno;
        }
    }
}
