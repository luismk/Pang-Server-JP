using GameServer.GameType;
using PangyaAPI.Utilities.BinaryModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.Game.Manager
{
    public class CaddieManager : Dictionary<uint/*ID*/, CaddieInfoEx>
    {
        public CaddieManager()
        {

        }

        public CaddieManager(Dictionary<uint/*ID*/, CaddieInfoEx> keys)
        {
            // this.(keys);    add array 
        }

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
                                       
        public CaddieInfoEx findCaddieById(uint _id)
        {
            return this.Values.FirstOrDefault(c => c.id == _id);
        }

        public CaddieInfoEx findCaddieByTypeid(uint _typeid)
        {
            return this.Values.FirstOrDefault(c => c._typeid == _typeid);
        }

        public CaddieInfoEx findCaddieByTypeidAndId(uint _typeid, uint _id)
        {
            return this.Values.FirstOrDefault(c => c.id == _id && c._typeid == _typeid);
        }
    }
}
