using System;
using System.Collections.Generic;
using System.Linq;
using Pangya_GameServer.GameType;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Game.Manager
{
    public class CardEquipManager : List<CardEquipInfoEx>
    {  
        public CardEquipInfo findCardEquipById(int _id)
        {
            return this.FirstOrDefault(c => c.id == _id);
        }

        public CardEquipInfo findCardEquipByTypeid(uint _typeid)
        {
            return this.FirstOrDefault(c => c._typeid == _typeid);
        }

        public CardEquipInfo findCardEquipByTypeidAndId(uint _typeid, int _id)
        {
            return this.FirstOrDefault(c => c.id == _id && c._typeid == _typeid);
        }
    }
}
