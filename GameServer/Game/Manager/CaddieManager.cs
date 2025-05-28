using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Pangya_GameServer.GameType;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Game.Manager
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
