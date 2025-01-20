using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities;
using System.Collections.Generic;
using GameServer.PangType;

namespace GameServer.Game.Manager
{
    public class WarehouseManager
    {
        protected multimap<uint/*ID*/, WarehouseItemEx> mp_wi;        // Tem que usar multimap aqui, para nao ficar realocando memória, uso o ponteiro de um element, para o item equipado
        protected List<List<WarehouseItemEx>> Values;
        public WarehouseManager(multimap<uint, WarehouseItemEx> _mp_wi)
        {
            mp_wi = _mp_wi;

            Values = mp_wi.SplitValues(20); //ChunkBy(this.ToList(), totalBySplit);
        }

        /// <summary>
        /// WAREHOUSE REBUILD ACRISIO OK
        /// </summary>
        /// <returns></returns>
        public byte[] GetWarehouseData()
        {
            var result = new PangyaBinaryWriter();
            result.Write(new byte[] { 0x73, 0x00 });
            result.Write((short)Values.Count);//desnecessario conta +1 por causa do ticker report
            result.Write((short)Values.Count);
            foreach (var item in Values.Split(1))
            {
                result.WriteStruct(item);
            }
            return result.GetBytes;
        }

       

    }
}
