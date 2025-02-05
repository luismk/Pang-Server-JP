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
    #region Struct MemorialShopCoinItem.sff
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class MemorialShopCoinItem : ICloneable
    {
        public MemorialShopCoinItem()
        {
        }
        public MemorialShopCoinItem(PangyaBinaryReader reader)
        {
            Active = reader.ReadUInt32();
            gacha_range = reader.Read<GachaRange>();
            ID = reader.ReadUInt32();
            Probabilities = reader.ReadUInt32();
            filter = reader.ReadInt32Array(5).ToArray();
        }
        public uint Active { get; set; }
        public uint ID { get; set; }
        public FilterCoinType CoinType { get; set; }//0 normal
        public uint Probabilities { get; set; }
        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
        public class GachaRange
        {
            public uint Number_Min { get; set; }
            public uint Number_Max { get; set; }
            public bool empty()
            {
                return Number_Min == 0 && Number_Max == 0;
            }
            public bool isBetweenGacha(uint _number)
            {
                return Number_Min <= _number && _number <= Number_Max;
            } 
        }
        [field: MarshalAs(UnmanagedType.Struct, SizeConst = 8)]
        public GachaRange gacha_range { get; set; }
        public FilterType ItemType { get; set; }//9-28
        public uint Sex { get; set; }//Count??8-32
        public uint Value_1 { get; set; }//7-36
        public uint Item { get; set; }//6-40
        public uint CharacterType { get; set; }//5-44

        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public int[] filter { get; set; }

        public bool hasFilter(int _filter)
        {
            var New_filter = new int[] { (int)ItemType, (int)Sex, (int)Value_1, (int)Item, (int)CharacterType, filter[0], filter[1], filter[2], filter[3], filter[4], };
            if (_filter == 0)
                return false;

            for (var i = 0u; i < 10; ++i)
                if (New_filter[i] == _filter)
                    return true;

            return false;
        }
        public bool emptyFilter()
        {
            var New_filter = new int[] { (int)ItemType, (int)Sex, (int)Value_1, (int)Item, (int)CharacterType, filter[0], filter[1], filter[2], filter[3], filter[4], };
            int count = 0;

            for (var i = 0u; i < 10; ++i)
                count += New_filter[i];

            return count == 0;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
    #endregion

}
