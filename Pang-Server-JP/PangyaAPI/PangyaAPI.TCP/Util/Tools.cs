using PangyaAPI.Utilities;
using System;
using System.Linq;
using System.Runtime.InteropServices;
namespace PangyaAPI.TCP.Util
{
    public static class Tools
    {
        public static T[] InitializeWithDefaultInstances<T>(int length) where T : class, new()
        {
            T[] array = new T[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = new T();
            }
            return array;
        }

        public static string[] InitializeStringArrayWithDefaultInstances(int length)
        {
            string[] array = new string[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = "";
            }
            return array;
        }

        public static T[] PadWithNull<T>(int length, T[] existingItems) where T : class
        {
            if (length > existingItems.Length)
            {
                T[] array = new T[length];

                for (int i = 0; i < existingItems.Length; i++)
                {
                    array[i] = existingItems[i];
                }

                return array;
            }
            else
                return existingItems;
        }

        public static T[] PadValueTypeArrayWithDefaultInstances<T>(int length, T[] existingItems) where T : struct
        {
            if (length > existingItems.Length)
            {
                T[] array = new T[length];

                for (int i = 0; i < existingItems.Length; i++)
                {
                    array[i] = existingItems[i];
                }

                return array;
            }
            else
                return existingItems;
        }

        public static T[] PadReferenceTypeArrayWithDefaultInstances<T>(int length, T[] existingItems) where T : class, new()
        {
            if (length > existingItems.Length)
            {
                T[] array = new T[length];

                for (int i = 0; i < existingItems.Length; i++)
                {
                    array[i] = existingItems[i];
                }

                for (int i = existingItems.Length; i < length; i++)
                {
                    array[i] = new T();
                }

                return array;
            }
            else
                return existingItems;
        }

        public static string[] PadStringArrayWithDefaultInstances(int length, string[] existingItems)
        {
            if (length > existingItems.Length)
            {
                string[] array = new string[length];

                for (int i = 0; i < existingItems.Length; i++)
                {
                    array[i] = existingItems[i];
                }

                for (int i = existingItems.Length; i < length; i++)
                {
                    array[i] = "";
                }

                return array;
            }
            else
                return existingItems;
        }

        public static void DeleteArray<T>(T[] array) where T : System.IDisposable
        {
            foreach (T element in array)
            {
                if (element != null)
                    element.Dispose();
            }
        }

        public static byte[] Clone(this byte[] sourceArray, int startIndex)
        {
            if (sourceArray == null || startIndex < 0 || startIndex >= sourceArray.Length)
                throw new ArgumentException("Invalid input parameters");

            int length = sourceArray.Length - startIndex;
            byte[] newArray = new byte[length];

            Array.Copy(sourceArray, startIndex, newArray, 0, length);

            return newArray;
        }
        public static T ByteArrayToStructure<T>(this byte[] bytes, int index) where T : class
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                IntPtr ptr = IntPtr.Add(handle.AddrOfPinnedObject(), index);
                return (T)Marshal.PtrToStructure(ptr, typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }
        public static byte[] ConvertArray(this object value)

        {
            int size = Marshal.SizeOf(value);
            byte[] arr = new byte[size];

            GCHandle handle = GCHandle.Alloc(arr, GCHandleType.Pinned);
            try
            {


                IntPtr ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(value, ptr, false);
                Marshal.Copy(ptr, arr, 0, size);
                Marshal.FreeHGlobal(ptr);
                return arr;
            }
            finally
            {
                handle.Free();
            }
        }

        public static Array Clear(this Array array)
        {
            Array.Clear(array, 0, array.Length);
            return array;
        }

        public static string ExceptionMessage(this Exception ex)
        {
            return "";
        }
        public static byte[] CopyBytes(this byte[] data, int Size)
        {
            var new_data = new byte[Size];

            Buffer.BlockCopy(data, 0, new_data, 0, Size);
            return new_data;
        }
        public static void CopyPropertiesTo<T, TU>(this T source, TU dest)
        {
            var sourceProps = typeof(T).GetProperties().Where(x => x.CanRead).ToList();
            var destProps = typeof(TU).GetProperties()
                    .Where(x => x.CanWrite)
                    .ToList();

            foreach (var sourceProp in sourceProps)
            {
                if (destProps.Any(x => x.Name == sourceProp.Name))
                {
                    var p = destProps.First(x => x.Name == sourceProp.Name);
                    if (p.CanWrite)
                    { // check if the property can be set or no.
                        p.SetValue(dest, sourceProp.GetValue(source, null), null);
                    }
                }

            }

        }
    }
}
