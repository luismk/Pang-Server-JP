using PangyaAPI.IFF.JP.Extensions;
using PangyaAPI.IFF.JP.Models.General;
using PangyaAPI.Utilities.BinaryModels;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
namespace PangyaAPI.IFF.JP.Models.Data
{
    #region class Mascot.iff
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class Mascot : IFFCommon
    {
        public Mascot() { }
        public Mascot(ref PangyaBinaryReader reader, uint strLen)
        {
            LoadFile(ref reader, strLen);
        }
        
        public void LoadFile(ref PangyaBinaryReader reader, uint strLen)
        {
            Load(ref reader, strLen);
            MPet = reader.ReadPStr(40);
            Texture1 = reader.ReadPStr(40);
            var prices = reader.ReadUInt16Array(5).ToArray();
            price = new byte[5];
            for (int i = 0; i < prices.Length; i++)
            {
                price[i] = (byte)prices[i];
            }
            Power = reader.ReadByte();
            Control = reader.ReadByte();
            Impact = reader.ReadByte();
            Spin = reader.ReadByte();
            Curve = reader.ReadByte();
            var Power_Drive = reader.ReadByte();
            var Drop_Rate = reader.ReadInt16();
            var Power_Gauge = reader.ReadInt16();
            var Pang_Rate = reader.ReadInt16();
            var Exp_Rate = reader.ReadInt16();
            var ItemSlot = reader.ReadByte();
            var Active_Message = reader.ReadByte();
            var Flag_Message = reader.ReadInt16();
            var Change_Price = reader.ReadUInt32();
            var Bonus_Pang = reader.ReadUInt16();
            var Bonus_Flag = reader.ReadUInt16();

            efeito = new Efeito()
            {
                power_drive = Power_Drive,
                drop_rate = Drop_Rate,
                exp_rate = Exp_Rate,
                item_slot = ItemSlot,
                pang_rate = Pang_Rate,
                power_gague = Power_Gauge
            };
            msg = new Mensagem()
            {
                active = Active_Message,
                change_price = Change_Price,
                flag = Flag_Message
            };

            bonus_pangya = new BonusPangya()
            {
                pang = Bonus_Pang,
                flag = Bonus_Flag
            };
        }


        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string MPet { get; set; }
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string Texture1 { get; set; }
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public byte[] price { get; set; }
        public byte Power { get; set; }
        public byte Control { get; set; }
        public byte Impact { get; set; }
        public byte Spin { get; set; }
        public byte Curve { get; set; }
        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 11)]
        public class Efeito
        {
            public short power_drive { get; set; }
            public short drop_rate { get; set; }
            public short power_gague { get; set; }
            public short pang_rate { get; set; }
            public short exp_rate { get; set; }
            public byte item_slot { get; set; }
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 7)]
        public class Mensagem
        {
            public byte active { get; set; }
            public short flag { get; set; }
            public uint change_price { get; set; }
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
        public class BonusPangya
        {
            public ushort pang { get; set; }
            public ushort flag { get; set; }
        }
        [field: MarshalAs(UnmanagedType.Struct)]
        public Efeito efeito { get; set; }
        [field: MarshalAs(UnmanagedType.Struct)]
        public Mensagem msg { get; set; }
        [field: MarshalAs(UnmanagedType.Struct)]
        public BonusPangya bonus_pangya { get; set; }           
    }
    #endregion

}
