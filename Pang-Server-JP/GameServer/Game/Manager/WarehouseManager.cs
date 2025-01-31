using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities;
using System.Collections.Generic;
using GameServer.PangType;
using System;
using System.Linq;

namespace GameServer.Game.Manager
{
    public class WarehouseManager   : Dictionary<uint/*ID*/, WarehouseItemEx>
    {                                                                              
        public WarehouseManager()
        {                                                                                 
        }

        /// <summary>
        /// WAREHOUSE REBUILD ACRISIO OK
        /// </summary>
        /// <returns></returns>
        public byte[] Build()
        {
            var p = new PangyaBinaryWriter();
            try
            {
                p.WriteUInt16((short)Count);
                p.WriteUInt16((short)Count);
                foreach (var item in Values)
                {
                    p.WriteBytes(item.Build());
                }
                return p.GetBytes;
            }
            catch (Exception)
            {
                return new byte[0];
            }
        }

        public byte[] GetInfo(uint _id)
        {
            var wi_info = findWarehouseItemById(_id);
            if (wi_info == null)
                return new byte[0];
            else
            {
                var p = new PangyaBinaryWriter();
                p.WriteBytes(wi_info.Build());
                return p.GetBytes;
            }
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
