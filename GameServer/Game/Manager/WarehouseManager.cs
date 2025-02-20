using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities;
using System.Collections.Generic;
using GameServer.GameType;
using System;
using System.Linq;           
namespace GameServer.Game.Manager
{
    public class WarehouseManager   : Dictionary<uint/*ID*/, WarehouseItemEx>
    {                                                                              
        public WarehouseManager()
        {                                                                                 
        }
                                
        protected PangyaBinaryWriter Build(List<WarehouseItemEx> list)
        {                                       
            try
            {                                   
                return PacketFunc.packet_func_sv.pacote073(list);
            }
            catch (Exception e)
            {
                return PacketFunc.packet_func_sv.pacote073(new List<WarehouseItemEx>());
            }
        }                                                                  

        public List<PangyaBinaryWriter> Build(uint itensPerPacket = 20)
        {
            var responses = new List<PangyaBinaryWriter>();
            if (Count * 196 < (1000 - 100))//envio normal
            {
                responses.Add(Build(Values.ToList()));
            }
            else
            {
                var splitList = this.Values.ToList().Split((int)itensPerPacket); //ChunkBy(this.ToList(), totalBySplit);

                //Percorre lista e adiciona ao resultado
                splitList.ForEach(lista => responses.Add(Build(lista)));
            }
            return responses;
        }                                 

        public List<KeyValuePair<uint, WarehouseItemEx>>  GetValues(uint _id)
        {
            return this.Where(c => c.Key == _id).ToList();
        }

        public bool Insert(uint _id, WarehouseItemEx item)
        {
             Add(_id, item);
            return true;
        }

        public WarehouseItemEx findWarehouseItemById(uint _id)
        {
            return this.Values.FirstOrDefault(c => c.id == _id);
        }

        public WarehouseItemEx findWarehouseItemByTypeid(uint _typeid)
        {
            return this.Values.FirstOrDefault(c => c._typeid == _typeid);
        }

        public WarehouseItemEx findWarehouseItemByTypeidAndId(uint _typeid, uint _id)
        {
            return this.Values.FirstOrDefault(c => c.id == _id && c._typeid == _typeid);
        }                     
    }
}
