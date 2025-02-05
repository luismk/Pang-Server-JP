using PangLib.IFF.JP.Models.General;
using System;
using System.Runtime.InteropServices;
using static PangLib.IFF.JP.Models.Data.CadieMagicBoxRandom;

namespace PangLib.IFF.JP.Models.Data
{
    /// <summary>
    /// Is Struct file CadieMagicBoxRandom.iff
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class CadieMagicBoxRandom  : ICloneable
    {
        public CadieMagicBoxRandom()
        {
            item_random = new ItemRandom();
        }

        public uint ID { get; set; }
        [field: MarshalAs(UnmanagedType.Struct)]
        public ItemRandom item_random { get; set; }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class ItemRandom
        {
            public uint ID { get; set; }

            public uint Qty { get; set; }

            public uint Rate { get; set; }
        }
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
