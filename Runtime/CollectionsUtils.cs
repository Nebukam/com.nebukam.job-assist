using Unity.Collections;
using System.Collections.Generic;

namespace Nebukam.JobAssist
{
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
            if(nativeArray.Length != length)
            {
                nativeArray.Dispose();
                nativeArray = new NativeArray<T>(length, alloc);
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
            if (nativeArray.Length < length)
            {
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
        /// <param name="sourceManagedArray"></param>
        /// <param name="targetNativeArray"></param>
        /// <returns>true if the size is unchanged, false if the NativeArray has been updated</returns>
        public static bool Copy<T>(ref T[] sourceManagedArray, ref NativeArray<T> targetNativeArray)
            where T : struct
        {
            int count = sourceManagedArray.Length;
            bool resized = MakeLength<T>(ref targetNativeArray, sourceManagedArray.Length);
            
            for(int i = 0; i < count; i++)
                targetNativeArray[i] = sourceManagedArray[i];
            
            return resized;
        }

        /// <summary>
        /// Copies the content of a managed list into a nativeArray
        /// Ensure the target native array has the same length as the source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceManagedList"></param>
        /// <param name="targetNativeArray"></param>
        /// <returns>true if the size is unchanged, false if the NativeArray has been updated</returns>
        public static bool Copy<T>(ref List<T> sourceManagedList, ref NativeArray<T> targetNativeArray)
            where T : struct
        {
            int count = sourceManagedList.Count;
            bool resized = MakeLength<T>(ref targetNativeArray, sourceManagedList.Count);

            for (int i = 0; i < count; i++)
                targetNativeArray[i] = sourceManagedList[i];

            return resized;
        }

        /// <summary>
        /// Copies the content of a managed list into a nativeArray
        /// Ensure the target native array has the same length as the source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceManagedList"></param>
        /// <param name="targetNativeList"></param>
        public static void Copy<T>(ref List<T> sourceManagedList, ref NativeList<T> targetNativeList)
            where T : struct
        {
            int count = sourceManagedList.Count;

            targetNativeList.Clear();
            if (targetNativeList.Capacity <= count) { targetNativeList.Capacity = count+1; }

            for (int i = 0; i < count; i++)
                targetNativeList.AddNoResize(sourceManagedList[i]);

        }

    }
}
