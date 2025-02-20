using GameServer.GameType;
using PangyaAPI.Utilities.BinaryModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Game.Manager
{
    public class CardManager : Dictionary<uint, CardInfo>
    {               
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
            var char_info = findCardById(_id);
            if (char_info == null)
                return new byte[0x19];
            else
            {
                var p = new PangyaBinaryWriter();
                p.WriteBytes(char_info.Build());
                return p.GetBytes;
            }
        }
        public CardInfo findCardById(uint _id)
        {
            return this.Values.FirstOrDefault(c => c.id == _id);
        }

        public CardInfo findCardByTypeid(uint _typeid)
        {
            return this.Values.FirstOrDefault(c => c._typeid == _typeid);
        }

        public CardInfo findCardByTypeidAndId(uint _typeid, uint _id)
        {
            return this.Values.FirstOrDefault(c => c.id == _id && c._typeid == _typeid);
        }
    }
}
