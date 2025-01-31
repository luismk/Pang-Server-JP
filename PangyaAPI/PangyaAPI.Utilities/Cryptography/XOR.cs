namespace PangyaAPI.Utilities.Cryptography
{
    /// <summary>
    /// Main XOR cipher class
    /// </summary>
    public static class XOR
    {
        /// <summary>
        /// XORs the given bytes with the given key
        /// </summary>
        /// <param name="data">Data to be ciphered</param>
        /// <param name="key">Key to cipher the data with</param>
        /// <returns>Cuphered data</returns>
        public static byte[] Cipher(byte[] data, uint key)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] ^= (byte)key;
            }

            return data;
        }



       public static string XOR_data(char[] Data, int DataSize, int Type)
        {
            if (Type == 1)
            {
                for (int i = 0; i < DataSize; i++)
                {
                    Data[i] ^= (char)0x71u;
                }
            }
            string Return = new string(Data);
            Return = Return.Substring(0, DataSize);
            return Return;
        }
    }
}
