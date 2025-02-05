using PangLib.IFF.JP.Extensions;
using PangLib.IFF.JP.Models.Flags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PangLib.IFF.JP.Models.Data
{
    #region Struct MemorialRareItem.iff
    [StructLayout(LayoutKind.Sequential, Pack = 4)]  
    public class MemorialShopRareItem  : ICloneable
    {
        public uint Active { get; set; }
        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
        public class Gacha
        {
            public uint Number { get; set; }
            public uint Count { get; set; }
        }
        [field: MarshalAs(UnmanagedType.Struct, SizeConst = 8)]
        public Gacha gacha { get; set; }
        public uint ID { get; set; }
        public uint Probabilities { get; set; }
        public MemorialRareType RareType { get; set; }// Tipo Raro, EX: -1 - 0 normal, 1 - 2 raro, 3 - 4 Super raro
        public FilterType ItemType { get; set; }//9-28
        public uint Sex { get; set; }//Count??8-32
        public uint Value_1 { get; set; }//7-36
        public uint Item { get; set; }//6-40
        public uint CharacterType { get; set; }//5-44

        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public int[] filter { get; set; }
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
        public byte[] Null_Bytes { get; set; }
        public MemorialShopRareItem()
        { }
        public MemorialShopRareItem(ref PangyaBinaryReader reader)
        {
            Active = reader.ReadUInt32();
            gacha = reader.Read<Gacha>();
            ID = reader.ReadUInt32();
            Probabilities = reader.ReadUInt32();
            RareType = (MemorialRareType)reader.ReadInt32();
            filter = reader.ReadInt32Array(5).ToArray();
            // Lendo os bytes nulos
            Null_Bytes = reader.ReadBytes(24);
        }
        public int[] getFilter()
        {
            var New_filter = new int[] { (int)ItemType, (int)Sex, (int)Value_1, (int)Item, (int)CharacterType, filter[0], filter[1], filter[2], filter[3], filter[4], };

            return New_filter;
        }
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
    #endregion
}
