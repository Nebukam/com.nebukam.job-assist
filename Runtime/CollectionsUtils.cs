using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Nebukam.JobAssist
{

    [BurstCompile]
    static public partial class CollectionsUtils
    {

        /// <summary>
        /// Ensure a NativeArray is of required size.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nativeArray"></param>
        /// <param name="length"></param>
        /// <param name="alloc"></param>
        /// <returns>true if the size is unchanged, false if the NativeArray has been updated</returns>
        public static bool MakeLength<T>(ref NativeArray<T> nativeArray, int length, Allocator alloc = Allocator.Persistent)
            where T : struct
        {
            if (!nativeArray.IsCreated
                || nativeArray.Length != length)
            {
                if (nativeArray.IsCreated)
                    nativeArray.Dispose();

                nativeArray = new NativeArray<T>(length, alloc);
                return false;
            }

            return true;

        }

        /// <summary>
        /// Ensure a NativeArray is of required size.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nativeHashMap"></param>
        /// <param name="length"></param>
        /// <param name="alloc"></param>
        /// <returns>true if the size is unchanged, false if the NativeArray has been updated</returns>
        public static bool MakeLength<TKey, TValue>(ref NativeHashMap<TKey, TValue> nativeHashMap, int length, Allocator alloc = Allocator.Persistent)
            where TKey : struct, IEquatable<TKey>
            where TValue : struct
        {
            if (!nativeHashMap.IsCreated
                || nativeHashMap.Count() != length)
            {
                if (nativeHashMap.IsCreated)
                    nativeHashMap.Dispose();

                nativeHashMap = new NativeHashMap<TKey, TValue>(length, alloc);
                return false;
            }

            return true;

        }

        /// <summary>
        /// Ensure a NativeArray is of required size.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nativeArray"></param>
        /// <param name="length"></param>
        /// <param name="alloc"></param>
        /// <returns>true if the size is unchanged, false if the NativeArray has been updated</returns>
        public static bool MakeLength<T>(ref T[] array, int length)
            where T : struct
        {
            if (array.Length != length)
            {
                array = new T[length];
                return false;
            }

            return true;

        }

        /// <summary>
        /// Ensure a NativeArray has at least a given size.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nativeArray"></param>
        /// <param name="length"></param>
        /// <param name="padding"></param>
        /// <param name="alloc"></param>
        /// <returns>true if the size is unchanged, false if the NativeArray has been updated</returns>
        public static bool EnsureMinLength<T>(ref NativeArray<T> nativeArray, int length, int padding = 0, Allocator alloc = Allocator.Persistent)
            where T : struct
        {
            if (!nativeArray.IsCreated
                || nativeArray.Length < length)
            {
                if(nativeArray.IsCreated)
                    nativeArray.Dispose();

                nativeArray = new NativeArray<T>(length + padding, alloc);
                return false;
            }

            return true;

        }

        /// <summary>
        /// Copies the content of a managed array into a nativeArray
        /// Ensure the target native array has the same length as the source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <returns>true if the size is unchanged, false if the NativeArray has been updated</returns>
        public static bool Copy<T>(T[] src, ref NativeArray<T> dest, Allocator alloc = Allocator.Persistent)
            where T : struct
        {
            int count = src.Length;
            bool resized = MakeLength<T>(ref dest, src.Length, alloc);
            NativeArray<T>.Copy(src, dest);
            return resized;
        }

        /// <summary>
        /// Copies the content of a NativeArray into a managed array
        /// Ensure the target native array has the same length as the source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <returns>true if the size is unchanged, false if the NativeArray has been updated</returns>
        public static bool Copy<T>(NativeArray<T> src, ref T[] dest)
            where T : struct
        {
            int count = src.Length;
            bool resized = dest.Length != count;
            if (resized) { dest = new T[count]; }
            NativeArray<T>.Copy(src, dest);
            return resized;
        }

        /// <summary>
        /// Copies the content of a NativeArray into a managed array
        /// Ensure the target native array has the same length as the source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <returns>true if the size is unchanged, false if the NativeArray has been updated</returns>
        public static bool Copy<T>(NativeArray<T> src, ref NativeArray<T> dest, Allocator alloc = Allocator.Persistent)
            where T : struct
        {
            int count = src.Length;
            bool resized = !dest.IsCreated || dest.Length != count;
            if (resized)
            {
                if(dest.IsCreated)
                    dest.Dispose();

                dest = new NativeArray<T>(count, alloc);
            }
            NativeArray<T>.Copy(src, dest);
            return resized;
        }

        /// <summary>
        /// Copies the content of a managed list into a nativeArray
        /// Ensure the target native array has the same length as the source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <returns>true if the size is unchanged, false if the NativeArray has been updated</returns>
        public static bool Copy<T>(List<T> src, ref NativeArray<T> dest, Allocator alloc = Allocator.Persistent)
            where T : struct
        {
            int count = src.Count;
            bool resized = MakeLength<T>(ref dest, src.Count, alloc);

            for (int i = 0; i < count; i++)
                dest[i] = src[i];

            return resized;
        }

        /// <summary>
        /// Copies the content of a managed list into a nativeArray
        /// Ensure the target native array has the same length as the source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        public static void Copy<T>(List<T> src, ref NativeList<T> dest, Allocator alloc = Allocator.Persistent)
            where T : struct
        {
            int count = src.Count;

            if (!dest.IsCreated)
            {
                dest = new NativeList<T>(count+1, alloc);
            }
            else
            {
                if (dest.Capacity <= count) { dest.Capacity = count + 1; }
            }

            for (int i = 0; i < count; i++)
                dest.AddNoResize(src[i]);

        }

        public static void FloodArray<T>(NativeArray<T> array, T value)
            where T : struct
        {
            new FloodArray<T> { array = array, value = value }.Run(array.Length);
        }

    }

    [BurstCompile]
    internal struct FloodArray<T> : Unity.Jobs.IJobParallelFor
        where T : struct
    {
        public NativeArray<T> array;
        public T value;
        public void Execute(int index)
        {
            array[index] = value;
        }
    }

}
