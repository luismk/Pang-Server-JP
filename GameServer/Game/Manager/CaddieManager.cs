using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Pangya_GameServer.GameType;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Game.Manager
{
    public class CaddieManager : Dictionary<int/*ID*/, CaddieInfoEx>
    {
        public CaddieManager()
        {

        }

        public CaddieManager(Dictionary<int/*ID*/, CaddieInfoEx> keys)
        {
            // this.(keys);    add array 
        }
         
        public CaddieInfoEx findCaddieById(int _id)
        {
            return this.Values.FirstOrDefault(c => c.id == _id);
        }

        public CaddieInfoEx findCaddieByTypeid(uint _typeid)
        {
            return this.Values.FirstOrDefault(c => c._typeid == _typeid);
        }

        public CaddieInfoEx findCaddieByTypeidAndId(uint _typeid, int _id)
        {
            return this.Values.FirstOrDefault(c => c.id == _id && c._typeid == _typeid);
        }
    }
}
