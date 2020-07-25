﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RaptorDB.Common
{
    public interface IKV<T, V>
    {
        bool TryGetValue(T key, out V val);
        int Count();
        IEnumerator<KeyValuePair<T, V>> GetEnumerator();
        void Add(T key, V value);
        T[] Keys();
        bool Remove(T key);
        void Clear();
        V GetValue(T key);
        // safesortedlist only
        //V GetValue(int index);
        //T GetKey(int index);
    }

    public class SafeDictionary<TKey, TValue> : IKV<TKey, TValue>
    {
        private readonly object _Padlock = new object();
        private readonly Dictionary<TKey, TValue> _Dictionary;

        public SafeDictionary(int capacity)
        {
            _Dictionary = new Dictionary<TKey, TValue>(capacity);
        }

        public SafeDictionary()
        {
            _Dictionary = new Dictionary<TKey, TValue>();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (_Padlock)
                return _Dictionary.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get
            {
                lock (_Padlock)
                    return _Dictionary[key];
            }
            set
            {
                lock (_Padlock)
                    _Dictionary[key] = value;
            }
        }

        public int Count()
        {
            lock (_Padlock) return _Dictionary.Count;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)_Dictionary).GetEnumerator();
        }

        public void Add(TKey key, TValue value)
        {
            lock (_Padlock)
            {
                if (_Dictionary.ContainsKey(key) == false)
                    _Dictionary.Add(key, value);
                else
                    _Dictionary[key] = value;
            }
        }

        public TKey[] Keys()
        {
            lock (_Padlock)
            {
                TKey[] keys = new TKey[_Dictionary.Keys.Count];
                _Dictionary.Keys.CopyTo(keys, 0);
                return keys;
            }
        }

        public bool Remove(TKey key)
        {
            if (key == null)
                return true;
            lock (_Padlock)
            {
                return _Dictionary.Remove(key);
            }
        }

        public void Clear()
        {
            lock (_Padlock)
                _Dictionary.Clear();
        }

        public TValue GetValue(TKey key)
        {
            lock (_Padlock)
                return _Dictionary[key];
        }
    }

    public class SafeSortedList<T, V> : IKV<T, V>
    {
        private object _padlock = new object();
        SortedList<T, V> _list = new SortedList<T, V>();

        public int Count()
        {
            lock (_padlock) return _list.Count;
        }

        public void Add(T key, V val)
        {
            lock (_padlock)
            {
                if (_list.ContainsKey(key) == false)
                    _list.Add(key, val);
                else
                    _list[key] = val;
            }
        }

        public bool Remove(T key)
        {
            if (key == null)
                return true;
            lock (_padlock)
                return _list.Remove(key);
        }

        public T GetKey(int index)
        {
            lock (_padlock)
                if (index < _list.Count)
                    return _list.Keys[index];
                else
                    return default(T);
        }

        public V GetValue(int index)
        {
            lock (_padlock)
                if (index < _list.Count)
                    return _list.Values[index];
                else
                    return default(V);
        }

        public T[] Keys()
        {
            lock (_padlock)
            {
                T[] keys = new T[_list.Keys.Count];
                _list.Keys.CopyTo(keys, 0);
                return keys;
            }
        }

        public V this[T key]
        {
            get
            {
                lock (_padlock)
                    return _list[key];
            }
            set
            {
                lock (_padlock)
                    _list[key] = value;
            }
        }

        public IEnumerator<KeyValuePair<T, V>> GetEnumerator()
        {
            return ((ICollection<KeyValuePair<T, V>>)_list).GetEnumerator();
        }

        public bool TryGetValue(T key, out V value)
        {
            lock (_padlock)
                return _list.TryGetValue(key, out value);
        }

        public void Clear()
        {
            lock (_padlock)
                _list.Clear();
        }

        public V GetValue(T key)
        {
            lock (_padlock)
                return _list[key];
        }
    }

    //------------------------------------------------------------------------------------------------------------------
    public static class FastDateTime
    {
        public static TimeSpan LocalUtcOffset;

        public static DateTime Now
        {
            get { return DateTime.SpecifyKind(DateTime.UtcNow + LocalUtcOffset, DateTimeKind.Local); }

        }

        static FastDateTime()
        {
            LocalUtcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
        }
    }
    //------------------------------------------------------------------------------------------------------------------

    public static class Helper
    {
        public static MurmurHash2Unsafe MurMur = new MurmurHash2Unsafe();
        public static int CompareMemCmp(byte[] left, byte[] right)
        {
            int c = left.Length;
            if (c > right.Length)
                c = right.Length;
            return memcmp(left, right, c);
        }

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int memcmp(byte[] arr1, byte[] arr2, int cnt);

        public static int ToInt32(byte[] value, int startIndex, bool reverse)
        {
            if (reverse)
            {
                byte[] b = new byte[4];
                Buffer.BlockCopy(value, startIndex, b, 0, 4);
                Array.Reverse(b);
                return ToInt32(b, 0);
            }

            return ToInt32(value, startIndex);
        }

        public static unsafe int ToInt32(byte[] value, int startIndex)
        {
            fixed (byte* numRef = &(value[startIndex]))
            {
                return *((int*)numRef);
            }
        }

        public static long ToInt64(byte[] value, int startIndex, bool reverse)
        {
            if (reverse)
            {
                byte[] b = new byte[8];
                Buffer.BlockCopy(value, startIndex, b, 0, 8);
                Array.Reverse(b);
                return ToInt64(b, 0);
            }
            return ToInt64(value, startIndex);
        }

        public static unsafe long ToInt64(byte[] value, int startIndex)
        {
            fixed (byte* numRef = &(value[startIndex]))
            {
                return *(((long*)numRef));
            }
        }

        public static short ToInt16(byte[] value, int startIndex, bool reverse)
        {
            if (reverse)
            {
                byte[] b = new byte[2];
                Buffer.BlockCopy(value, startIndex, b, 0, 2);
                Array.Reverse(b);
                return ToInt16(b, 0);
            }
            return ToInt16(value, startIndex);
        }

        public static unsafe short ToInt16(byte[] value, int startIndex)
        {
            fixed (byte* numRef = &(value[startIndex]))
            {
                return *(((short*)numRef));
            }
        }

        public static unsafe byte[] GetBytes(long num, bool reverse)
        {
            byte[] buffer = new byte[8];
            fixed (byte* numRef = buffer)
            {
                *((long*)numRef) = num;
            }
            if (reverse)
                Array.Reverse(buffer);
            return buffer;
        }

        public static unsafe byte[] GetBytes(int num, bool reverse)
        {
            byte[] buffer = new byte[4];
            fixed (byte* numRef = buffer)
            {
                *((int*)numRef) = num;
            }
            if (reverse)
                Array.Reverse(buffer);
            return buffer;
        }

        public static unsafe byte[] GetBytes(short num, bool reverse)
        {
            byte[] buffer = new byte[2];
            fixed (byte* numRef = buffer)
            {
                *((short*)numRef) = num;
            }
            if (reverse)
                Array.Reverse(buffer);
            return buffer;
        }

        public static byte[] GetBytes(string s)
        {
            return Encoding.UTF8.GetBytes(s); // TODO : change to unicode ??
        }

        public static string GetString(byte[] buffer, int index, short length)
        {
            return Encoding.UTF8.GetString(buffer, index, length); // TODO : change to unicode ??
        }
    }
}
