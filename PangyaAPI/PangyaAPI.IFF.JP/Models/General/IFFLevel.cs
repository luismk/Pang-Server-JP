using System;
using System.Data;
using System.Runtime.InteropServices;
using PangyaAPI.IFF.JP.Models.Flags;
namespace PangyaAPI.IFF.JP.Models.General
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 1)]
    public class IFFLevel
    {
        [field: MarshalAs(UnmanagedType.U1, SizeConst = 1)]
        private byte _level { get; set; }//ler somente esse ;D

        public bool GoodLevel(byte _stlevel)
        {
            if (is_max && _stlevel <= level)
                return true;
            else if (!is_max && _stlevel >= level)
                return true;

            return false;
        }
        public byte level
        {
            get
            {
                return (byte)(_level & 0x7F);
            }
            set
            {
                if (value < 0 || (value & 0x7F) > 127)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "O valor de 'level' deve estar entre 0 e 127.");
                }
                _level = value;
            }
        }
        /// <summary>
        /// set value in level max, true = 70, false = other value
        /// </summary>
        public bool is_max
        {
            get { 
                if (_level == 70)
                    return true;
                else if ((_level & 128) == 128)
                    return true;
                return false;
            }
            set
            {
                if (value)
                {
                    // Set the bit 128 in level
                    level = (byte)(_level | 128);
                }
                else
                {
                    // Clear the bit 128 in level
                    level = (byte)(_level & ~128);
                }
            }
        }
    }
}
