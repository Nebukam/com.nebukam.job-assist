using System;
using Unity.Jobs;
using Unity.Collections;

namespace Nebukam.JobAssist
{
    public static class Extensions
    {

        /// <summary>
        /// Call Complete() on a JobHandle only if the job IsCompleted = true.
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool TryComplete(this JobHandle @this)
        {
            if (@this.IsCompleted) { @this.Complete(); return true; }
            return false;
        }

        /// <summary>
        /// Extremely inneficient "remove at" method for NativeList<T>
        /// Usefull for debug & making sure algorithms are working as intented
        /// Shouldn't be used in production.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static T RemoveAt<T>(this NativeList<T> @this, int index)
            where T : struct
        {
            int length = @this.Length;
            T val = @this[index];

            for (int i = index; i < length-1; i++)
                @this[i] = @this[i + 1];

            @this.ResizeUninitialized(length-1);
            return val;

        }

        /// <summary>
        /// Extremely inneficient "remove at" method for NativeList<T>
        /// Usefull for debug & making sure algorithms are working as intented
        /// Shouldn't be used in production.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static T RemoveAt<T>(ref NativeList<T> @this, int index)
            where T : struct
        {

            int length = @this.Length;
            T val = @this[index];

            for (int i = index; i < length-1; i++)
                @this[i] = @this[i + 1];


            @this.ResizeUninitialized(length - 1);
            return val;
        }

        /// <summary>
        /// Checks whether a NativeMultiHashMap as a given value associated to a given key
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="this"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Checks whether a NativeMultiHashMap as a given value associated to a given key
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="this"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Removes a single value from the list associated to the a key
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="this"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Remove<TKey, TValue>(this NativeMultiHashMap<TKey, TValue> @this, ref TKey key, ref TValue value)
            where TKey : struct, IEquatable<TKey>
            where TValue : struct, IEquatable<TValue>
        {
            if (!@this.Contains(ref key, ref value)) { return false; }

            NativeList<TValue> values = new NativeList<TValue>(5, Allocator.Temp);
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
            values.Dispose();

            return true;
        }

        /// <summary>
        /// Removes a single value from the list associated to the a key
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="this"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Remove<TKey, TValue>(ref NativeMultiHashMap<TKey, TValue> @this, ref TKey key, ref TValue value)
            where TKey : struct, IEquatable<TKey>
            where TValue : struct, IEquatable<TValue>
        {
            if (!@this.Contains(ref key, ref value)) { return false; }

            NativeList<TValue> values = new NativeList<TValue>(5, Allocator.Temp);
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
            values.Dispose();
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

        /// <summary>
        /// Return a list containing all values associated to a given key.
        /// If no value is found, returns an empty list.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="this"></param>
        /// <param name="key"></param>
        /// <param name="alloc"></param>
        /// <param name="capacity"></param>
        /// <returns></returns>
        public static NativeList<TValue> GetValues<TKey, TValue>(this NativeMultiHashMap<TKey, TValue> @this, ref TKey key, Allocator alloc, int capacity = 5)
            where TKey : struct, IEquatable<TKey>
            where TValue : struct, IEquatable<TValue>
        {

            NativeList<TValue> list = new NativeList<TValue>(capacity, alloc);

            NativeList<TValue> values = new NativeList<TValue>(5, Allocator.Temp);
            NativeMultiHashMapIterator<TKey> it;

            TValue result;
            if (@this.TryGetFirstValue(key, out result, out it))
            {
                list.Add(result);
                while (@this.TryGetNextValue(out result, ref it))
                {
                    list.Add(result);
                }
            }

            return list;
        }

        /// <summary>
        /// Return a list containing all values associated to a given key.
        /// If no value is found, returns an empty list.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="this"></param>
        /// <param name="key"></param>
        /// <param name="alloc"></param>
        /// <param name="capacity"></param>
        /// <returns></returns>
        public static NativeList<TValue> GetValues<TKey, TValue>(ref NativeMultiHashMap<TKey, TValue> @this, ref TKey key, Allocator alloc, int capacity = 5)
            where TKey : struct, IEquatable<TKey>
            where TValue : struct, IEquatable<TValue>
        {

            NativeList<TValue> list = new NativeList<TValue>(capacity, alloc);

            NativeList<TValue> values = new NativeList<TValue>(5, Allocator.Temp);
            NativeMultiHashMapIterator<TKey> it;

            TValue result;
            if (@this.TryGetFirstValue(key, out result, out it))
            {
                list.Add(result);
                while (@this.TryGetNextValue(out result, ref it))
                {
                    list.Add(result);
                }
            }

            return list;
        }

        /// <summary>
        /// Push value associated with a given key to a given list.
        /// Return the number of values added.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="this"></param>
        /// <param name="key"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static int PushValues<TKey, TValue>(this NativeMultiHashMap<TKey, TValue> @this, ref TKey key, ref NativeList<TValue> list)
            where TKey : struct, IEquatable<TKey>
            where TValue : struct, IEquatable<TValue>
        {

            NativeList<TValue> values = new NativeList<TValue>(5, Allocator.Temp);
            NativeMultiHashMapIterator<TKey> it;

            int resultCount = 0;
            TValue result;
            if (@this.TryGetFirstValue(key, out result, out it))
            {
                list.Add(result);
                resultCount++;
                while (@this.TryGetNextValue(out result, ref it))
                {
                    list.Add(result);
                    resultCount++;
                }
            }

            return resultCount;
        }

        /// <summary>
        /// Push value associated with a given key to a given list.
        /// Return the number of values added.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="this"></param>
        /// <param name="key"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static int PushValues<TKey, TValue>(ref NativeMultiHashMap<TKey, TValue> @this, ref TKey key, ref NativeList<TValue> list)
            where TKey : struct, IEquatable<TKey>
            where TValue : struct, IEquatable<TValue>
        {

            NativeList<TValue> values = new NativeList<TValue>(5, Allocator.Temp);
            NativeMultiHashMapIterator<TKey> it;

            int resultCount = 0;
            TValue result;
            if (@this.TryGetFirstValue(key, out result, out it))
            {
                list.Add(result);
                resultCount++;
                while (@this.TryGetNextValue(out result, ref it))
                {
                    list.Add(result);
                    resultCount++;
                }
            }

            return resultCount;
        }

    }
}
