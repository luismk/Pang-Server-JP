using PangyaAPI.Cryptor.PangyaTableKey;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PangyaAPI.Cryptor.Maker
{
    internal class CryptClientBuild
    {
        public void Decrypt(ref uint source)
        {
            //code sent by: Hsreina
            byte[] pval = BitConverter.GetBytes(source);
            int i, index;
            for (i = 0, index = 0; i < PublicTableKey.CryptTableDeserialize.Length; i++)
            {
                pval[index] ^= PublicTableKey.CryptTableDeserialize[i];
                index = (index == 3) ? 0 : ++index;
            }
            source = BitConverter.ToUInt32(pval, 0);
        }

    }
}
