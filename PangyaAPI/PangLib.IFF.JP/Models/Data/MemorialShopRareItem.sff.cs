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
    public class MemorialShopRareItemSff  : ICloneable
    {
        public int coin_tipo;
        public int coin_typeid;
        public int coin_probabilidade;
        public int item_tipo;
        public int item_typeid;
        public int item_probabilidade;
        public int item_gacha_number;
        public int item_qntd;
        public int item_dup;
        public MemorialShopRareItemSff()
        { }
         
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
    #endregion
}
