using System;
using System.Runtime.InteropServices;
using System.Text;
using PangyaAPI.IFF.JP.Models.Flags;
using PangyaAPI.IFF.JP.Models.General;

namespace PangyaAPI.IFF.JP.Models.Data
{
    #region Struct CadieMagicBox.iff
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class CadieMagicBox : ICloneable
    {
        /// <summary>
        /// Index
        /// </summary>
        public uint Index { get; set; }//index
        public uint Active { get; set; }//valido
        public CadieBoxSetor Page { get; set; }//showOnPage
        public CadieBoxEnum BoxType { get; set; }//
        public uint Level { get; set; }//okay
        public uint ProdItem { get; set; }
        /// <summary>
        /// Type Index
        /// </summary>
        public uint ID { get; set; }
        public uint Total { get; set; }
        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 32)]
        public class Packege
        {
            [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public uint[] ID { get; set; }
            [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public uint[] Qntd { get; set; }
            public Packege()
            {
                ID = new uint[4];
                Qntd = new uint[4];
            }
        }
        [field: MarshalAs(UnmanagedType.Struct)]
        public Packege box_packege { get; set; } = new Packege();
        public uint Box_Random_ID { get; set; }
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        byte[] NameInBytes { get; set; }
        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 32)]
        public class _Date
        {
            [field: MarshalAs(UnmanagedType.Struct, SizeConst = 16)]
            public IFFTime Start { get; set; }
            [field: MarshalAs(UnmanagedType.Struct, SizeConst = 16)]
            public IFFTime End { get; set; }
            public _Date()
            {
                End = new IFFTime();
                Start = new IFFTime();
            }
            public bool Check()
            {
                return (DateTime.Compare(Start.Time, DateTime.Now) < 0) & (DateTime.Compare(End.Time, DateTime.Now) > 0);
            }
        }
        [field: MarshalAs(UnmanagedType.Struct)]
        public _Date date { get; set; } = new _Date();
        public string Name { get => Encoding.GetEncoding("Shift_JIS").GetString(NameInBytes).Replace("\0", ""); set => NameInBytes = Encoding.GetEncoding("Shift_JIS").GetBytes(value.PadRight(40, '\0')); }


        public CadieMagicBox()
        {
            // Initialize properties with default values
            Index = 0;
            Active = 0;
            Page = new CadieBoxSetor(); // Ensure this class has a parameterless constructor
            BoxType = CadieBoxEnum.PART; // Ensure this enum has a default value
            Level = 0;
            ProdItem = 0;
            ID = 0;
            Total = 0;
            box_packege = new Packege(); // Initialize the nested Packege class
            Box_Random_ID = 0;
            NameInBytes = new byte[40]; // Initialize the byte array
            date = new _Date(); // Initialize the nested _Date class
        }
        public object Clone()
        {
            return MemberwiseClone();
        }

    }
    #endregion
}
