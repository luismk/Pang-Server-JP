using System;
using System.Collections.Generic;
using System.Linq;
using PangyaAPI.Network.Pangya_St;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Game.Manager
{
    public class CharacterManager : Dictionary<uint/*ID*/, CharacterInfo>
    {
        public CharacterManager()
        {

        }

        public CharacterManager(Dictionary<uint/*ID*/, CharacterInfo> keys)
        {
            // this.(keys);    add array 
        }
  
        public CharacterInfo findCharacterById(uint _id)
        {
            return this.Values.FirstOrDefault(c => c.id == _id);
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
