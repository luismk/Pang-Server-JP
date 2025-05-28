using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangya_GameServer.GameType
{
    public enum CARD_TYPE : uint
    {
        T_NORMAL,
        T_RARE,
        T_SUPER_RARE,
        T_SECRET
    }

    public class Card
    {
        public void clear()
        { 
        }
        public uint _typeid = new uint();
        public uint prob = new uint(); // Probabilidade
        public CARD_TYPE tipo; // tipo, Normal, Rare, Super Rare, Secreto
    }

    public class CardPack
    {
        public CardPack(uint _ul = 0u)
        {
            clear();
        }
        public CardPack(uint __typeid,
            uint _num, byte _volume)
        { 
            this._typeid = __typeid;
             this.num = _num;
             this.volume = _volume;
        } 
        public void clear()
        {
            rate = new Rate();
            card = new List<Card>();
            if (card.Count > 0)
            {
                card.Clear();
            }

            _typeid = 0u;
            num = 0u;
            volume = 0;
        }
        public class Rate
        {
            public void clear()
            {
                value = new ushort[4];
            }
            public ushort[] value = new ushort[4]; // Normal, Rare, Super Rare, Secret
        }
        public uint _typeid = new uint();
        public uint num = new uint(); // Número de card(s) que esse pack dá
        public byte volume; // Volume do Card Pack, Vol 1, 2, 3, 4, 5 etc
        public Rate rate = new Rate(); // Rate, N, R, SR, SC
        public List<Card> card = new List<Card>(); // Cards
    }

    public class LoloCardCompose
    {
        public void clear()
        {
            _typeid = new uint[3];
        }
        public ulong pang = new ulong();
        public uint[] _typeid = new uint[3];
    }

    public class LoloCardComposeEx : LoloCardCompose
    {
        public LoloCardComposeEx(uint _ul = 0u)
        {
            clear();
        }
        public new void clear()
        { 
        }
        public byte tipo;
    }

}
