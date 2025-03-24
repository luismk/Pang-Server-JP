using PangLib.IFF.JP.Extensions;
using PangLib.IFF.JP.Models.General;
using PangyaAPI.Utilities.BinaryModels;
using System;
using System.IO;
using System.Runtime.InteropServices;
namespace PangLib.IFF.JP.Models.Data
{                     
    #region Struct Caddie.iff
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class Caddie : IFFCommon
    {
        public uint Salary { get; set; }
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string MPet { get; set; }
        [field: MarshalAs(UnmanagedType.Struct, SizeConst = 10)]
        public IFFStats Stats { get; set; }
        public ushort Point { get; set; }
               

        public Caddie()
        { }

        public Caddie(ref PangyaBinaryReader read)
        {
            LoadFile(ref read);
        }

        public void LoadFile(ref PangyaBinaryReader reader)
        {
            Load(ref reader, 40);
            Salary = reader.ReadUInt32();
            MPet = reader.ReadPStr(40);
            Stats = reader.Read<IFFStats>();
            Point = reader.ReadUInt16();
        }    
    }
    #endregion
}
