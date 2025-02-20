using GameServer.GameType;
using PangyaAPI.Utilities.BinaryModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace GameServer.Game.Manager
{
    public class MascotManager : Dictionary<uint/*ID*/, MascotInfoEx>
    {
        public MascotManager()
        {

        }

        public MascotManager(Dictionary<uint/*ID*/, MascotInfoEx> keys)
        {
            // this.(keys);    add array 
        }

        public byte[] Build()
        {
            var p = new PangyaBinaryWriter();
            try
            {                                   
                p.WriteByte((byte)(Count & 0xFF));

                foreach (var item in Values)
                    p.WriteBytes(item.Build());

                return p.GetBytes;
            }
            catch (Exception)
            {
                return new byte[0];
            }
        }
                                                                      
        public MascotInfo findMascotById(uint _id)
        {
            return this.Values.FirstOrDefault(c => c.id == _id);
        }

        public MascotInfo findMascotByTypeid(uint _typeid)
        {
            return this.Values.FirstOrDefault(c => c._typeid == _typeid);
        }

        public MascotInfo findMascotByTypeidAndId(uint _typeid, uint _id)
        {
            return this.Values.FirstOrDefault(c => c.id == _id && c._typeid == _typeid);
        }
    }
}
