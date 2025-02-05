using PangLib.IFF.JP.Models.General;
using System;
using System.Runtime.InteropServices;
using PangLib.IFF.JP.Extensions;
using System.Linq;
namespace PangLib.IFF.JP.Models.Data
{
    #region Struct SetItem.iff
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Packege
    {
        public uint Total { get; set; }
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public uint[] Item_TypeID { get; set; }
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public ushort[] Item_Qty { get; set; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class SetItem : IFFCommon
    {
        public uint Total { get; set; }
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public uint[] Item_TypeID { get; set; }
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public ushort[] Item_Qty { get; set; }
        [field: MarshalAs(UnmanagedType.Struct, SizeConst = 10)]
        public IFFStats Stats { get; set; } // aqui deve ser algum tempo
        public ushort Point { get; set; }
        public uint TypeSet => (uint)((ID & ~0xFC000000) >> 21);
        public uint CharacterType
        {
            get
            {
                for (int i = 0; i < Item_TypeID.Length; i++)
                {
                    if (Item_TypeID[i] > 0 && Utils.GetItemGroup(Item_TypeID[i]) == 2)
                    {
                        return (uint)((Item_TypeID[i] & 0x3fc0000) / Math.Pow(2.0, 18.0));
                    }
                }
                // Retorna um valor padrão se nenhuma condição for satisfeita
                return uint.MaxValue;
            }
        }

        public int getCharacter(bool memorial = false)
        {
            if (memorial)
            {        
                switch (CharacterType)
                {
                    case 0:             
                    case 2:        
                    case 4:     
                    case 7:     
                    case 11:
                    case 13:
                        return 17; //man 
                    case 1: // weman
                    case 3: // weman
                    case 5:
                    case 6:
                    case 8:
                    case 9:
                    case 10:   //Spika                                                    
                    case 12:  //Hana Renew                                               
                    case 14: //CC Renew
                        return 18;
                    default:
                        return 0;
                }                         
            }
            else
            {
                switch (CharacterType)
                {
                    case 0:
                        return 1; //nuri classic
                    case 1:
                        return 2;
                    case 2:
                        return 3;
                    case 3:
                        return 4;
                    case 4:
                        return 5;
                    case 5:
                        return 6;
                    case 6:
                        return 7;
                    case 7:
                        return 8;
                    case 8:
                        return 9;
                    case 9:
                        return 10;//nell
                    case 10:   //Spika 
                        return 11;
                    case 11:   //Nuri Renew
                        return 12;
                    case 12:  //Hana Renew
                        return 13;
                    case 13: //Azer Renew
                        return 0;// 13 azer Renew, porem tem que retornar 0;
                    case 14: //CC Renew
                        return 14;
                    default:
                        return 0;
                }
            }
        }

        public SetItem()
        { }
        public SetItem(ref PangyaBinaryReader reader, uint strlen)
        {
            Load(ref reader, strlen);
            Total = reader.ReadUInt32();
            Item_TypeID = reader.ReadUInt32Array(10).ToArray();
            Item_Qty = reader.ReadUInt16Array(10).ToArray();
            Stats = reader.Read<IFFStats>();//deve ser o mesmo esquema do item.cs
            Point = reader.ReadUInt16();
        }

        public string GetQntSet(int idx)
        {
            var part = Utils.GetItemGroup(Item_TypeID[idx]) == 2;
            var _char = (Utils.GetItemGroup(Item_TypeID[idx]) == 1);
            var club = (Utils.GetItemGroup(Item_TypeID[idx]) == 4);
            var caddie = (Utils.GetItemGroup(Item_TypeID[idx]) == 7);
            var mascot = (Utils.GetItemGroup(Item_TypeID[idx]) == 16);
            if (part || _char || club || caddie || mascot)
            {
                if (Item_Qty[idx] == 0)
                {
                    return "1";
                }
            }
            return Convert.ToString(Item_Qty[idx]);//retorna 1 por causa do set item
        }

        public string GetIDSet(int idx)
        {
            var _id = Item_TypeID[idx];
            return Convert.ToString(_id <= 0 ? 0 : _id);//retorna 1 por causa do set item
        }

        public void SetQntSet(int idx, string text)
        {
            var part = Utils.GetItemGroup(Item_TypeID[idx]) == 2;
            var _char = (Utils.GetItemGroup(Item_TypeID[idx]) == 1);
            var club = (Utils.GetItemGroup(Item_TypeID[idx]) == 4);
            var caddie = (Utils.GetItemGroup(Item_TypeID[idx]) == 7);
            var mascot = (Utils.GetItemGroup(Item_TypeID[idx]) == 16);
            if (part || _char || club|| caddie || mascot)
            {
                if (ushort.Parse(text) == 1)
                {
                    text = "0";
                }  
            }
            Item_Qty[idx] = ushort.Parse(text);
        }

        public void SetIDSet(int idx, string text)
        {     
            Item_TypeID[idx] = uint.Parse(text);
        }

        public uint[] GetIDS()
        {
            return Item_TypeID.Where(c=> c!= 0).ToArray();         
        }

        public bool IsSet()
        {
            bool result = false;
            for (int i = 0; i < Item_TypeID.Length; i++)
            {
                if (Item_TypeID[i] > 0 && Utils.GetItemGroup(Item_TypeID[i]) == 2)
                {
                    result = true;
                }  
            }
            return result;
        }
             
    }
    #endregion     
}
