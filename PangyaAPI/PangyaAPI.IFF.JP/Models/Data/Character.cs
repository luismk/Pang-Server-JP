using System.Runtime.InteropServices;
using PangyaAPI.IFF.JP.Models.General;
namespace PangyaAPI.IFF.JP.Models.Data
{

    #region Struct Character.iff
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class Character : IFFCommon
    {
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string MPet { get; set; }
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string Texture1 { get; set; }
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string Texture2 { get; set; }
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string Texture3 { get; set; }
        public ushort Power { get; set; }
        public ushort Control { get; set; }
        public ushort Impact { get; set; }
        public ushort Spin { get; set; }
        public ushort Curve { get; set; }
        public byte NumberParts { get; set; }
        public byte NumberAcessory { get; set; }
        public int ClubType { get; set; }
        public float ClubScale { get; set; }
        public byte PowerSlot { get; set; }
        public byte ControlSlot { get; set; }
        public byte ImpactSlot { get; set; }
        public byte SpinSlot { get; set; }
        public byte CurveSlot { get; set; }
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 43)]
        public string Camera { get; set; }
    }
    #endregion
}
