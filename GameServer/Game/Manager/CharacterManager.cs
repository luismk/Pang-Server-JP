using GameServer.GameType;
using PangyaAPI.Network.Pangya_St;
using PangyaAPI.Utilities.BinaryModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace GameServer.Game.Manager
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

        public byte[] Build()
        {
            var p = new PangyaBinaryWriter();
            try
            {
                p.WriteInt16((short)Count);
                p.WriteInt16((short)Count);  
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

        public byte[] GetInfo(CharacterInfo char_info)
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.Write(char_info._typeid);
                p.Write(char_info.id);
                p.Write(char_info.default_hair);
                p.Write(char_info.default_shirts);
                p.Write(char_info.gift_flag);
                p.Write(char_info.purchase);
                for (var Index = 0; Index < 24; Index++)
                    p.Write(char_info.parts_typeid[Index]);
                for (var Index = 0; Index < 24; Index++)
                    p.Write(char_info.parts_id[Index]);
                p.Write(char_info.Blank, 216); //deve ser algum objeto ainda nao terminado
                for (int i = 0; i < 5; i++)
                    p.WriteUInt32(char_info.auxparts[i]);
                for (int i = 0; i < 4; i++)
                    p.WriteUInt32(char_info.cut_in[i]);
                for (int i = 0; i < 5; i++)
                    p.WriteUInt32(char_info.pcl[i]);

                p.WriteUInt32(char_info.mastery);
                for (int i = 0; i < 4; i++)
                    p.WriteUInt32(char_info.Card_Caddie[i]);
                for (int i = 0; i < 4; i++)
                    p.WriteUInt32(char_info.Card_Character[i]);
                for (int i = 0; i < 4; i++)
                    p.WriteUInt32(char_info.Card_NPC[i]);
                return p.GetBytes;
            }
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
