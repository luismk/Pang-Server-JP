﻿using PangLib.IFF.JP.Extensions;
using System.Linq;
using System.Runtime.InteropServices;

namespace PangLib.IFF.JP.Models.Data
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class ClubSetWorkShopLevelUpProb
    {
        public uint tipo;
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public uint[] c { get; set; }
        public ClubSetWorkShopLevelUpProb()
        { }
        public ClubSetWorkShopLevelUpProb(PangyaBinaryReader reader)
        {
            tipo = reader.ReadUInt32();
            c = reader.ReadUInt32Array(6).ToArray();
        }
    }
}