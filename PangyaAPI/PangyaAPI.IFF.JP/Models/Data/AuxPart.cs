using System;
using System.Runtime.InteropServices;
using PangyaAPI.IFF.JP.Models.General;
using PangyaAPI.Utilities.BinaryModels;
namespace PangyaAPI.IFF.JP.Models.Data
{
    #region Struct AuxPart.iff
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class AuxPart : IFFCommon
    {
        public AuxPart() { }
        public AuxPart(PangyaBinaryReader reader)
        {
            Load(ref reader, 40);
            Price1Day = reader.ReadUInt16();
            Price7Day = reader.ReadUInt16();
            Price15Day = reader.ReadUInt16();
            Price30Day = reader.ReadUInt16();
            Price365Day = reader.ReadUInt16();
            Power = reader.ReadByte();
            Control = reader.ReadByte();
            Impact = reader.ReadByte();
            Spin = reader.ReadByte();
            Curve = reader.ReadByte();
            PowerSlot = reader.ReadByte();
            ControlSlot = reader.ReadByte();
            ImpactSlot = reader.ReadByte();
            SpinSlot = reader.ReadByte();
            CurveSlot = reader.ReadByte();
            Power_Drive = reader.ReadUInt16();
            Drop_Rate = reader.ReadUInt16();
            Power_Gauge = reader.ReadUInt16();
            Pang_Rate = reader.ReadUInt16();
            Exp_Rate = reader.ReadUInt16();
            ItemSlot = reader.ReadUInt16();
            Bonus_Pang = reader.ReadUInt16();
            Bonus_Flag = reader.ReadUInt16();
        }

        public ushort Price1Day { get; set; }
        public ushort Price7Day { get; set; }
        public ushort Price15Day { get; set; }
        public ushort Price30Day { get; set; }
        public ushort Price365Day { get; set; }
        public byte Power { get; set; }
        public byte Control { get; set; }
        public byte Impact { get; set; }
        public byte Spin { get; set; }
        public byte Curve { get; set; }
        public byte PowerSlot { get; set; }
        public byte ControlSlot { get; set; }
        public byte ImpactSlot { get; set; }
        public byte SpinSlot { get; set; }
        public byte CurveSlot { get; set; }
        public UInt16 Power_Drive { get; set; }
        public UInt16 Drop_Rate { get; set; }
        public UInt16 Power_Gauge { get; set; }
        public UInt16 Pang_Rate { get; set; }
        public UInt16 Exp_Rate { get; set; }
        public UInt16 ItemSlot { get; set; }
        public ushort Bonus_Pang { get; set; }
        public ushort Bonus_Flag { get; set; }

        // Método para identificar o tipo de um typeid
        public string IdentifyType()
        {
            byte result;

            result = (byte)((ID & ~0xFC000000) >> 21);

            return result.ToString();
        }
        public int IdentifyRing()
        {
            byte result;

            result = (byte)((ID & ~0xFC000000) >> 21);

            return result;
        }

        // Método auxiliar para verificar se o typeId corresponde a um padrão específico
        private bool IsPatternMatch(uint pattern)
        {
            // Extrai os bits relevantes e compara com o padrão
            return (ID & pattern) == pattern;
        }
    }
    #endregion
}
