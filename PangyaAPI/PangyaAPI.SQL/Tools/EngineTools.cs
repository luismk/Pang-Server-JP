using System;
using System.Collections;

namespace PangyaAPI.SQL
{
    public static class EngineTools
    {
        public static bool IsNullOrEmpty<T>(T array)
        {
            return array == null;
        }

        public static bool IsNullOrEmpty<T>(T[] array)
        {
            return array == null || array.Length == 0;
        }

        public static uint ToUInt(this bool value)
        {
            return Convert.ToUInt32(value);
        }

        public static BitArray PadToFullByte(BitArray bits)
        {
            BitArray array = new BitArray(4096, false);
            if (bits.Count > 0)
            {
                for (int i = 0; i < bits.Count; i++)
                {
                    if ((bits.Count > 8) && (i < 8))
                    {
                        array.Set(i, bits[i]);
                    }
                }
            }
            return array;
        }

        public static bool Contains(this string s, string value)
        {
            return s.Contains(value);
        }
        public static string ToStrings(this sbyte s)
        {
            return $"'{s}'";
        }

        public static string ToStrings(this byte s)
        {
            return $"'{s}'";
        }

        public static string ToStrings(this short s)
        {
            return $"'{s}'";
        }

        public static string ToStrings(this ushort s)
        {
            return $"'{s}'";
        }

        public static string ToStrings(this int s)
        {
            return $"'{s}'";
        }

        public static string ToStrings(this bool s)
        {
            return $"'{s}'";
        }

        public static string ToStrings(this string s)
        {
            return $"'{s}'";
        }

        public static string ToStrings(this uint s)
        {
            return $"'{s}'";
        }

        public static string ToStrings(this long s)
        {
            return $"'{s}'";
        }

        public static string ToStrings(this ulong s)
        {
            return $"'{s}'";
        }

        public static string ToStrings(this float s)
        {
            return $"'{s}'";
        }

        public static string ToStrings(this double s)
        {
            return $"'{s}'";
        }

        public static string ToStrings(this decimal s)
        {
            return $"'{s}'";
        }

        public static string ToStrings(this char s)
        {
            return $"'{s}'";
        }

    }
}
