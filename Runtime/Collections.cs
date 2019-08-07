using System;
using Unity.Jobs;
using Unity.Collections;

namespace Nebukam.JobAssist
{
    public static class Collections
    {

        /// <summary>
        /// Extremely inneficient "remove at" method for NativeList<T>
        /// Usefull for debug & making sure algorithms are working as intented
        /// Shouldn't be used in production.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static void RemoveAt<T>(this NativeList<T> @this, int index)
            where T : struct
        {

            int length = @this.Length;
            for (int i = index; i < length; i++)
                @this[i] = @this[i + 1];

            @this.ResizeUninitialized(length-1);

        }

        public static void RemoveAt<T>(ref NativeList<T> @this, int index)
            where T : struct
        {

            int length = @this.Length;
            for (int i = index; i < length; i++)
                @this[i] = @this[i + 1];

            @this.ResizeUninitialized(length - 1);

        }

        public static bool Contains<TKey, TValue>(this NativeMultiHashMap<TKey, TValue> @this, ref TKey key, ref TValue value )
            where TKey : struct, IEquatable<TKey>
            where TValue : struct, IEquatable<TValue>
        {
            NativeMultiHashMapIterator<TKey> it;
            TValue result;
            if (@this.TryGetFirstValue(key, out result, out it))
            {
                if (result.Equals(value)) { return true; }
                while (@this.TryGetNextValue(out result, ref it))
                {
                    if (result.Equals(value)) { return true; }
                }
            }
            return false;
        }

        public static bool Contains<TKey, TValue>(ref NativeMultiHashMap<TKey, TValue> @this, ref TKey key, ref TValue value)
            where TKey : struct, IEquatable<TKey>
            where TValue : struct, IEquatable<TValue>
        {
            NativeMultiHashMapIterator<TKey> it;
            TValue result;
            if (@this.TryGetFirstValue(key, out result, out it))
            {
                if (result.Equals(value)) { return true; }
                while (@this.TryGetNextValue(out result, ref it))
                {
                    if (result.Equals(value)) { return true; }
                }
            }
            return false;
        }

        public static bool AddOnce<TKey, TValue>(this NativeMultiHashMap<TKey, TValue> @this, ref TKey key, ref TValue value)
            where TKey : struct, IEquatable<TKey>
            where TValue : struct, IEquatable<TValue>
        {
            if(@this.Contains(ref key, ref value)) { return false; }
            @this.Add(key, value);
            return true;
        }

        public static bool AddOnce<TKey, TValue>(ref NativeMultiHashMap<TKey, TValue> @this, ref TKey key, ref TValue value)
            where TKey : struct, IEquatable<TKey>
            where TValue : struct, IEquatable<TValue>
        {
            if (@this.Contains(ref key, ref value)) { return false; }
            @this.Add(key, value);
            return true;
        }

        public static bool Remove<TKey, TValue>(this NativeMultiHashMap<TKey, TValue> @this, ref TKey key, ref TValue value)
            where TKey : struct, IEquatable<TKey>
            where TValue : struct, IEquatable<TValue>
        {
            if (!@this.Contains(ref key, ref value)) { return false; }

            NativeList<TValue> values = new NativeList<TValue>();
            NativeMultiHashMapIterator<TKey> it;
            TValue result;
            if (@this.TryGetFirstValue(key, out result, out it))
            {
                if (result.Equals(value)) { } else { values.Add(result); }
                while (@this.TryGetNextValue(out result, ref it))
                {
                    if (result.Equals(value)) { } else { values.Add(result); }
                }
            }

            @this.Remove(key);
            for (int i = 0, count = values.Length; i < count; i++) { @this.Add(key, values[i]); }

            return true;
        }

        public static bool Remove<TKey, TValue>(ref NativeMultiHashMap<TKey, TValue> @this, ref TKey key, ref TValue value)
            where TKey : struct, IEquatable<TKey>
            where TValue : struct, IEquatable<TValue>
        {
            if (!@this.Contains(ref key, ref value)) { return false; }

            NativeList<TValue> values = new NativeList<TValue>();
            NativeMultiHashMapIterator<TKey> it;
            TValue result;
            if (@this.TryGetFirstValue(key, out result, out it))
            {
                if (result.Equals(value)) { } else { values.Add(result); }
                while (@this.TryGetNextValue(out result, ref it))
                {
                    if (result.Equals(value)) { } else { values.Add(result); }
                }
            }

            @this.Remove(key);
            for (int i = 0, count = values.Length; i < count; i++) { @this.Add(key, values[i]); }

            return true;
        }

        public static bool Contains<TValue>(this NativeList<TValue> @this, ref TValue value)
            where TValue : struct, IEquatable<TValue>
        {
            for(int i = 0, count = @this.Length; i < count; i++ )
                if (@this[i].Equals(value)) { return true; }
            return false;
        }

        public static bool AddOnce<TValue>(this NativeList<TValue> @this,ref TValue value)
            where TValue : struct, IEquatable<TValue>
        {
            if (@this.Contains(ref value)) { return false; }
            @this.Add(value);
            return true;
        }



    }
}
