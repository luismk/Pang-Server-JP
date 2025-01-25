using GameServer.PangType;
using PangyaAPI.Network.Pangya_St;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Game.Manager
{
    public class CharacterManager : SortedList<uint/*ID*/, CharacterInfoEx>
    {
        public CharacterManager()
        {
            
        }
        public CharacterInfo findCharacterById(int _id)
        {
            return this.Values.First(c=> c.id == _id);
        }

        public CharacterInfo findCharacterByTypeid(int _typeid)
        {
            return this.Values.First(c => c._typeid == _typeid);
        }

        public CharacterInfo findCharacterByTypeidAndId(int _typeid, int _id)
        {
            return this.Values.First(c => c.id == _id && c._typeid == _typeid);
        }        
    }
}
