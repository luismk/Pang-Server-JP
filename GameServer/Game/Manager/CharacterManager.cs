using GameServer.PangType;
using PangyaAPI.Network.Pangya_St;
using PangyaAPI.Utilities.BinaryModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.Game.Manager
{
    public class CharacterManager : Dictionary<uint/*ID*/, CharacterInfoEx>
    {
        public CharacterManager()
        {
            
        }

        public CharacterManager(Dictionary<uint/*ID*/, CharacterInfoEx> keys)
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
                    p.WriteStruct(item, new CharacterInfo());
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
           var char_info = findCharacterById(_id);
            if (char_info == null)
                return new byte[0];
            else
            {
                var p = new PangyaBinaryWriter();
                p.WriteStruct(char_info, new CharacterInfo());
                return p.GetBytes;
            }
        }
        public CharacterInfo findCharacterById(uint _id)
        {
            return this.Values.FirstOrDefault(c=> c.id == _id);
        }

        public CharacterInfo findCharacterByTypeid(uint _typeid)
        {
            return this.Values.FirstOrDefault(c => c._typeid == _typeid);
        }

        public CharacterInfo findCharacterByTypeidAndId(uint _typeid, uint _id)
        {
            return this.Values.FirstOrDefault(c => c.id == _id && c._typeid == _typeid);
        }        
    }
}
