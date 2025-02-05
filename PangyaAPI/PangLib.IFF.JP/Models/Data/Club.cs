﻿using PangLib.IFF.JP.Extensions;
using PangLib.IFF.JP.Models.General;
using System.IO;
using System.Runtime.InteropServices;
namespace PangLib.IFF.JP.Models.Data
{

    #region Struct Club.iff

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class Club : IFFCommon
    {
      
        public Club()
        { }

        public Club(ref PangyaBinaryReader read)
        {
            LoadFile(ref read);
        }

        public void LoadFile(ref PangyaBinaryReader reader)
        {
            Load(ref reader, 40);
            MPet = reader.ReadPStr(40);
            ClubType = reader.ReadUInt16();
            Stats = reader.Read<IFFStats>();
        }

        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string MPet { get; set; }
        public ushort ClubType { get; set; }

        [field: MarshalAs(UnmanagedType.Struct)]
        public IFFStats Stats { get; set; }
    }
    #endregion

}
