using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Pangya_GameServer.Cmd;
using Pangya_GameServer.GameType;
using Pangya_GameServer.PacketFunc;
using Pangya_GameServer.Session;
using PangyaAPI.IFF.JP.Extensions;
using PangyaAPI.IFF.JP.Models.Flags;
using PangyaAPI.IFF.JP.Models.General;
using PangyaAPI.Network.Pangya_St;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.SQL;
using PangyaAPI.SQL.Manager;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;
using static Pangya_GameServer.GameType._Define;
namespace Pangya_GameServer.Game.Manager
{
    /// <summary>
    /// Manipulation Add, Check, Remove Items
    /// </summary>
    public class item_manager
    {
        public class RetAddItem
        {
            public enum TYPE : int
            {
                T_INIT_VALUE = -5,
                T_ERROR,
                TR_SUCCESS_WITH_ERROR,
                TR_SUCCES_PANG_AND_EXP_AND_CP_WITH_ERROR,
                TR_SUCCESS_PANG_AND_EXP_AND_CP_POUCH_WITH_ERROR,
                T_SUCCESS_PANG_AND_EXP_AND_CP_POUCH,
                T_SUCCESS
            }

            public RetAddItem()
            {
                clear();
            }
            public void clear()
            {

                if (!fails.Any())
                {
                    fails.Clear();
                }
                type = TYPE.T_INIT_VALUE;
            }
            public List<stItem> fails = new List<stItem>();
            public TYPE type = new TYPE();
        }
        public item_manager()
        {
        }

        static void INIT_COMMOM_BUYITEM(ref stItem _item, BuyItem _bi, IFFCommon item)
        {
            _item.id = _bi.id;
            _item._typeid = _bi._typeid;
            _item.date.date.sysDate[0] = new PangyaTime(item.date.Start.Year, item.date.Start.Month, item.date.Start.Day, item.date.Start.Hour, item.date.Start.Minute, item.date.Start.Second, item.date.Start.MilliSecond);//check start later
            _item.date.date.sysDate[1] = new PangyaTime(item.date.End.Year, item.date.End.Month, item.date.End.Day, item.date.End.Hour, item.date.End.Minute, item.date.End.Second, item.date.End.MilliSecond);//check End later
            _item.date.active = Convert.ToUInt32(item.date.active);//check active date later
            _item.price = item.Shop.Price;
            _item.desconto = item.Shop.DiscountPrice;
            _item.qntd = _bi.qntd;
            _item.is_cash = (byte)(item.Shop.flag_shop.IsCash == true ? 1 : 0);
            _item.type = 2; /*aqui é o valor padrão, mas outros iff pode mexer nele depois*/
        }

        static void BEGIN_INIT_BUYITEM(player_info _pi, ref stItem _item, BuyItem _bi, bool _gift_opt, bool _chk_lvl, IFFCommon item)
        {
            if (item != null)
            {
                CHECK_LEVEL_ITEM(_pi, ref _item, _gift_opt, _chk_lvl, item);
                CHECK_IS_GIFT(_pi, ref _item, _gift_opt, _chk_lvl, item);
                INIT_COMMOM_BUYITEM(ref _item, _bi, item);
            }
            else
                message_pool.push(new message("Item nao encontrado. Typeid: " + (_bi._typeid), type_msg.CL_FILE_LOG_AND_CONSOLE));
        }

        static void CHECK_LEVEL_ITEM(player_info _pi, ref stItem _item, bool _gift_opt, bool _chk_lvl, IFFCommon item)
        {
            if (!_gift_opt && !_chk_lvl && !item.Level.GoodLevel((byte)_pi.level))
            {
                message_pool.push(new message("[Log] Player[UID=" + (_pi.uid)

                        + "] nao tem o level[value=" + (item.Level) + "] necessario para comprar esse item[TYPEID=" + (item.ID) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
                _item._typeid = 0;
                return;
            }
        }

        static void CHECK_IS_GIFT(player_info _pi, ref stItem _item, bool _gift_opt, bool _chk_lvl, IFFCommon item)
        {
            if (_gift_opt && !sIff.getInstance().IsGiftItem(item.ID))
            {
                message_pool.push(new message("[Log] Player[UID=" + (_pi.uid) + "] tentou presentear um item que não pode ser presenteado.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                _item._typeid = 0;
                return;
            }
        }

        static int STDA_TRANSLATE_FLAG_TIME(int flagTime, int value)
        {
            if (flagTime == 0x20 || flagTime == 2 || flagTime == 3)
                return value * 60 * 60; // Hour
            else if (flagTime == 0x10 || flagTime == 0x40 || flagTime == 0x60 || flagTime == 1 || flagTime == 4 || flagTime == 6)
                return value * 24 * 60 * 60; // Day
            else if (flagTime == 0x50 || flagTime == 5)
                return value * 30 * 24 * 60 * 60; // 30 Days
            else
                return 0;
        }

        static int STDA_TRANSLATE_FLAG_TIME_TO_HOUR(int flagTime, int value)
        {
            return STDA_TRANSLATE_FLAG_TIME(flagTime, value) / 3600;
        }

        //gets
        public static List<stItem> getItemOfSetItem(Player _session, uint _typeid, bool _shop, int _chk_level)
        {
            if (!isSetItem(_typeid))
            {
                throw new exception("[item_manager::getItemOfSetItem][Error] item[TYPEID=" + Convert.ToString(_typeid) + "] not is a valid SetItem. Player: " + Convert.ToString(_session.m_pi.uid), ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE._ITEM_MANAGER,
                    1, 0));
            }

            List<stItem> v_item = new List<stItem>();
            stItem item = new stItem();
            BuyItem bi = new BuyItem();

            var set_item = sIff.getInstance().findSetItem(_typeid);

            if (set_item == null)
            {
                throw new exception("[item_manager::getItemOfSetItem][Error] item[TYPEID=" + Convert.ToString(_typeid) + "] nao foi encontrado. Player: " + Convert.ToString(_session.m_pi.uid), ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE._ITEM_MANAGER,
                    2, 0));
            }

            for (var i = 0u; i < (set_item.Item_TypeID.Length); ++i)
            {
                if (set_item.Item_TypeID[i] != 0)
                {
                    item = new stItem();
                    bi = new BuyItem();


                    bi.id = -1;
                    bi._typeid = set_item.Item_TypeID[i];
                    bi.qntd = set_item.Item_Qty[i];

                    initItemFromBuyItem(_session.m_pi,
                        ref item, bi, _shop, 0, 0,
                        _chk_level);

                    if (item._typeid != 0)
                    {
                        v_item.Add(item);
                    }
                    else
                    {
                        throw new exception("[item_manager::getItemOfSetItem][Error] erro ao inicializar item[TYPEID=" + Convert.ToString(set_item.Item_TypeID[i]) + "]. Player: " + Convert.ToString(_session.m_pi.uid), ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE._ITEM_MANAGER,
                            25, 0));
                    }
                }
            }

            return new List<stItem>(v_item);
        }

        //// Init Item From Buy --Gift opt-- Item
        public static void initItemFromBuyItem(PlayerInfo _pi, ref stItem _item, BuyItem _bi, bool _shop, int _option, int _gift_opt = 0, int _chk_lvl = 0)
        {
            // Limpa o _item
            _item.clear();
            switch (sIff.getInstance()._getItemGroupIdentify(_bi._typeid))
            {
                case IFF_GROUP.CHARACTER:
                    {
                        var item = sIff.getInstance().findCharacter(_bi._typeid);

                        BEGIN_INIT_BUYITEM(_pi, ref _item, _bi, _gift_opt.IsTrue(), _chk_lvl.IsTrue(), item);
                    }
                    break;
                case IFF_GROUP.PART:
                    {
                        var item = sIff.getInstance().findPart(_bi._typeid);

                        BEGIN_INIT_BUYITEM(_pi, ref _item, _bi, _gift_opt.IsTrue(), _chk_lvl.IsTrue(), item);

                        if (_option == 1/*Rental*/ && item.RentPang > 0)
                        {
                            _item.price = (uint)item.RentPang;
                            _item.is_cash = 0;  // Pang, por que é rental
                            _item.STDA_C_ITEM_TIME = 7;     // 7 dias	// no original é no C[3] o tempos
                            _item.flag = 0x60;  // dias Rental(acho)
                                                // time tipo 6 rental, 4, 2,
                            _item.flag_time = 6;
                        }
                        else if (_bi.time > 0)
                        {   // Roupa de tempo do CadieCauldron

                            if (item.Shop.flag_shop.time_shop.active)
                                message_pool.push(new message("[item_manager::initItemFromBuyItem][WARNIG] Player[UID=" + (_pi.uid)
                                        + "] inicializou Part[TYPEID=" + (_bi._typeid) + "] com tempo[VALUE=" + (_bi.time)
                                        + "], mas no IFF_STRUCT do server ele nao eh um item por tempo. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                            _item.STDA_C_ITEM_TIME = (ushort)_bi.time;
                            _item.flag = 0x20;
                            _item.flag_time = 2;
                        }

                        // UCC
                        if (item.IsUCC())
                            _item.flag = 5;

                        _item.type_iff = (byte)item.type_item;

                    }
                    break;
                case IFF_GROUP.CLUBSET:
                    {
                        var item = sIff.getInstance().findClubSet(_bi._typeid);

                        BEGIN_INIT_BUYITEM(_pi, ref _item, _bi, _gift_opt.IsTrue(), _chk_lvl.IsTrue(), item);

                        if (_bi.time > 0)
                        {

                            _item.qntd = 1;
                            _item.flag = 0x20;
                            _item.flag_time = 2/*Hora*/;
                            _item.STDA_C_ITEM_TIME = (ushort)(_bi.time * 24);
                        }
                    }
                    break;
                case IFF_GROUP.BALL:
                    {

                        var item = sIff.getInstance().findBall(_bi._typeid);

                        BEGIN_INIT_BUYITEM(_pi, ref _item, _bi, _gift_opt.IsTrue(), _chk_lvl.IsTrue(), item);
                        for (int i = 0; i < _item.c.Length; i++)
                        {
                            _item.c[i] = item.Stats.getSlot[i];
                        }
                        if (_chk_lvl.IsTrue())
                            _item.STDA_C_ITEM_QNTD = (ushort)_item.qntd;
                        else if (_item.qntd != _item.STDA_C_ITEM_QNTD)
                        {

                            _item.STDA_C_ITEM_QNTD = (ushort)_item.qntd;

                            _item.qntd = 1;

                        }
                        else
                        {

                            if (!_chk_lvl.IsTrue() && _item.qntd > 0 && _item.STDA_C_ITEM_QNTD != 0)
                                _item.qntd /= (uint)_item.STDA_C_ITEM_QNTD;
                        }
                    }
                    break;
                case IFF_GROUP.ITEM:
                    {
                        var item = sIff.getInstance().findItem(_bi._typeid);

                        BEGIN_INIT_BUYITEM(_pi, ref _item, _bi, _gift_opt.IsTrue(), _chk_lvl.IsTrue(), item);

                        // essa regra é para os itens que é 1 item, mas tem mais quantidade que vai ser add, 1 item normal, mas ele vem 10, fica no STDA_C_ITEM_QNTD
                        if (_item.qntd > 0 && _item.STDA_C_ITEM_QNTD != 0 && (_item.STDA_C_ITEM_QNTD == 1 || _item.STDA_C_ITEM_QNTD == _item.qntd))
                            _item.qntd /= (uint)_item.STDA_C_ITEM_QNTD;

                        // Copia C[] do IFF::Item para o _item
                        for (int i = 0; i < _item.c.Length; i++)
                        {
                            _item.c[i] = item.Stats.getSlot[i];
                        }
                        // Tem preço de tempo
                        var empty_price = sIff.getInstance().EMPTY_ARRAY_PRICE(_item.c);

                        if (_bi.time > 0 && !empty_price && sIff.getInstance().getEnchantSlotStat(_item._typeid) == 0x21/*Item por tempo, enchante é ~0xFC000000 >> 20*/)
                        {

                            if (item.Shop.flag_shop.time_shop.active && item.Shop.flag_shop.time_shop.dia > 0)
                            {

                                switch (_bi.time)
                                {
                                    case 1:
                                        if (_item.is_cash.IsTrue() ? _bi.cookie == _item.c[0] : _bi.pang == _item.c[0])
                                            _item.price = (uint)_item.c[0];
                                        break;
                                    case 7:
                                        if (_item.is_cash.IsTrue() ? _bi.cookie == _item.c[1] : _bi.pang == _item.c[1])
                                            _item.price = (uint)(uint)_item.c[1];
                                        break;
                                    case 15:
                                        if (_item.is_cash.IsTrue() ? _bi.cookie == _item.c[2] : _bi.pang == _item.c[2])
                                            _item.price = (uint)(uint)_item.c[2];
                                        break;
                                    case 30:
                                        if (_item.is_cash.IsTrue() ? _bi.cookie == _item.c[3] : _bi.pang == _item.c[3])
                                            _item.price = (uint)(uint)_item.c[3];
                                        break;
                                    case 365:
                                        if (_item.is_cash.IsTrue() ? _bi.cookie == _item.c[4] : _bi.pang == _item.c[4])
                                            _item.price = (uint)(uint)_item.c[4];
                                        break;
                                    default:
                                        _bi.time = 1; // 1 dia, Coloca o menor
                                        break;
                                }

                                _item.qntd = 1;//item.Shop.flag_shop.time_shop.uc_time_start;
                                _item.flag = 0x20;
                                _item.flag_time = 2/*Hora*/;
                                _item.STDA_C_ITEM_TIME = (ushort)(_bi.time * 24); // Premium Ticket

                                if (_bi.time > 365)
                                    message_pool.push(new message("[WARNING] Player[UID=" + (_pi.uid) + "]. Queria colocar mais[request="
                                            + (_bi.time) + "] que 365 dia na compra do Premium Ticket. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                            }

                        }
                        else if (_bi.time == 0 && !empty_price && sIff.getInstance().getEnchantSlotStat(_item._typeid) == 0x21/*Item por tempo, enchante é ~0xFC000000 >> 20*/ && _bi.qntd > 0)
                        {

                            message_pool.push(new message("[item_manager::initItemFromBuyItem][WARNING] Player[UID=" + (_pi.uid)
                                    + "] tentou inicializar Item[TYPEID=" + (_bi._typeid) + "] sem tempo no jogo e no IFF_STRUCT ele tem tempo. Hacker ou Command GM.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                            _bi.time = (short)(_bi.qntd > 365 ? 365 : _bi.qntd);

                            // Qntd tem que ser 1 por que o item é por tempo
                            if (_bi.qntd > 1)
                                _item.qntd = _bi.qntd = 1;

                            if (item.Shop.flag_shop.time_shop.active && item.Shop.flag_shop.time_shop.dia > 0)
                            {

                                switch (_bi.time)
                                {
                                    case 1:
                                        if (_item.is_cash.IsTrue() ? _bi.cookie == _item.c[0] : _bi.pang == _item.c[0])
                                            _item.price = (uint)_item.c[0];
                                        break;
                                    case 7:
                                        if (_item.is_cash.IsTrue() ? _bi.cookie == _item.c[1] : _bi.pang == _item.c[1])
                                            _item.price = (uint)_item.c[1];
                                        break;
                                    case 15:
                                        if (_item.is_cash.IsTrue() ? _bi.cookie == _item.c[2] : _bi.pang == _item.c[2])
                                            _item.price = (uint)_item.c[2];
                                        break;
                                    case 30:
                                        if (_item.is_cash.IsTrue() ? _bi.cookie == _item.c[3] : _bi.pang == _item.c[3])
                                            _item.price = (uint)_item.c[3];
                                        break;
                                    case 365:
                                        if (_item.is_cash.IsTrue() ? _bi.cookie == _item.c[4] : _bi.pang == _item.c[4])
                                            _item.price = (uint)_item.c[4];
                                        break;
                                    default:
                                        _bi.time = 1; // 1 dia, coloca o menor
                                        break;
                                }

                                _item.qntd = 1;//item.Shop.flag_shop.time_shop.uc_time_start;
                                _item.flag = 0x20;
                                _item.flag_time = 2/*Hora*/;
                                _item.STDA_C_ITEM_TIME = (ushort)(_bi.time * 24); // Premium Ticket

                                if (_bi.time > 365)
                                    message_pool.push(new message("[WARNING] Player[UID=" + (_pi.uid) + "]. Queria colocar mais[request="
                                            + (_bi.time) + "] que 365 dia na compra do Premium Ticket. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                            }
                        }

                        if (/*item.Shop.flag_shop.uFlagShop.IFF_item_pack && */(item.ID == 0x1A000081 || item.ID == 0x1A000082 || item.ID == 0x1A0003B3/*Event gacha Sale Pack*/))
                        {
                            _item._typeid = 0x1A000080; // Typeid do Coupon Gacha Single, por que os outros é item pack, em um item
                        }

                        if (_item.STDA_C_ITEM_QNTD <= 0 || _item.STDA_C_ITEM_QNTD < (int)_item.qntd)
                            _item.STDA_C_ITEM_QNTD = (ushort)_item.qntd;

                        if ((!_shop || _item.qntd == 0) && _item.STDA_C_ITEM_QNTD != _bi.qntd)
                        {
                            _item.STDA_C_ITEM_QNTD = (ushort)_bi.qntd;
                            _item.qntd = _bi.qntd;
                        }

                        if (sIff.getInstance().IsItemEquipable(_bi._typeid))
                        {   // Equiável
                        }
                        else
                        {   // Passivo

                        }

                    }
                    break;
                case IFF_GROUP.CADDIE:
                    {
                        var item = sIff.getInstance().findCaddie(_bi._typeid);

                        BEGIN_INIT_BUYITEM(_pi, ref _item, _bi, _gift_opt.IsTrue(), _chk_lvl.IsTrue(), item);

                        if (item.Salary > 0)
                        {
                            _item.date_reserve = 30;    // 30 dias   
                            _item.flag = 0x20;  // Time de dias( acho que seja o 0x20, não lembro mais)
                            _item.flag_time = 2;
                            for (int i = 0; i < _item.c.Length; i++)
                            {
                                _item.c[i] = item.Stats.getSlot[i];
                            }
                            _item.STDA_C_ITEM_TIME = (ushort)_item.date_reserve;    // Caddie depois que add, só colocar o time novamente
                        }
                    }
                    break;
                case IFF_GROUP.CAD_ITEM: //falta
                    {

                        var item = sIff.getInstance().findCaddieItem(_bi._typeid);

                        BEGIN_INIT_BUYITEM(_pi, ref _item, _bi, _gift_opt.IsTrue(), _chk_lvl.IsTrue(), item);

                        // Aqui não precisa ver se tem time_limit e time_start, so tem que verificar se tem o item.price[0~4]
                        var empty_price = sIff.getInstance().EMPTY_ARRAY_PRICE(item.price);

                        if (_bi.time > 0 && !empty_price)
                        {

                            switch (_bi.time)
                            {   // Dias
                                case 1:
                                    if (_item.is_cash.IsTrue() ? _bi.cookie == item.price[0] : _bi.pang == item.price[0])
                                        _item.price = (uint)item.price[0];
                                    break;
                                case 7:
                                    if (_item.is_cash.IsTrue() ? _bi.cookie == item.price[1] : _bi.pang == item.price[1])
                                        _item.price = (uint)item.price[1];
                                    break;
                                case 15:
                                    if (_item.is_cash.IsTrue() ? _bi.cookie == item.price[2] : _bi.pang == item.price[2])
                                        _item.price = (uint)item.price[2];
                                    break;
                                case 30:
                                    if (_item.is_cash.IsTrue() ? _bi.cookie == item.price[3] : _bi.pang == item.price[3])
                                        _item.price = (uint)item.price[3];
                                    break;
                            }

                            _item.STDA_C_ITEM_QNTD = 1;// item.Shop.flag_shop.time_shop.uc_time_start;
                            _item.flag_time = 2;
                            _item.flag = 0x20;
                            _item.STDA_C_ITEM_TIME = (ushort)((ushort)_bi.time * 24); // Horas

                        }
                        else if (_bi.time > 0 && (item.Shop.flag_shop.time_shop.active || item.Shop.flag_shop.time_shop.dia > 0))
                        {

                            message_pool.push(new message("[item_manager::initItemFromBuyItem][WARNING] Player[UID=" + (_pi.uid)
                                    + "] inicializou Caddie Item[TYPEID=" + (_bi._typeid) + "] com tempo no jogo e no IFF_STRUCT, mas ele nao tem os precos de tempo no IFF_STRUCT. Box ou Comando GM", type_msg.CL_FILE_LOG_AND_CONSOLE));

                            // Qntd tem que ser 1 por que o item é por tempo
                            if (_bi.qntd > 1)
                                _item.qntd = _bi.qntd = 1;

                            _item.STDA_C_ITEM_QNTD = 1;// item.Shop.flag_shop.time_shop.uc_time_start;
                            _item.flag_time = 4;
                            _item.flag = 0x40;
                            _item.STDA_C_ITEM_TIME = (ushort)_bi.time;  // Dias

                        }
                        else if (_bi.time == 0 && !empty_price && _bi.qntd > 0)
                        {

                            message_pool.push(new message("[item_manager::initItemFromBuyItem][WARNING] Player[UID=" + (_pi.uid)
                                    + "] tentou inicializar Caddie Item[TYPEID=" + (_bi._typeid) + "] sem tempo no jogo e no IFF_STRUCT ele tem tempo. Hacker ou Command GM.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                            _bi.time = (short)(_bi.qntd > 30 ? 30 : _bi.qntd);

                            // Qntd tem que ser 1 por que o item é por tempo
                            if (_bi.qntd > 1)
                                _item.qntd = _bi.qntd = 1;

                            switch (_bi.time)
                            {   // Dias
                                case 1:
                                    if (_item.is_cash.IsTrue() ? _bi.cookie == item.price[0] : _bi.pang == item.price[0])
                                        _item.price = (uint)item.price[0];
                                    break;
                                case 7:
                                    if (_item.is_cash.IsTrue() ? _bi.cookie == item.price[1] : _bi.pang == item.price[1])
                                        _item.price = (uint)item.price[1];
                                    break;
                                case 15:
                                    if (_item.is_cash.IsTrue() ? _bi.cookie == item.price[2] : _bi.pang == item.price[2])
                                        _item.price = (uint)item.price[2];
                                    break;
                                case 30:
                                    if (_item.is_cash.IsTrue() ? _bi.cookie == item.price[3] : _bi.pang == item.price[3])
                                        _item.price = (uint)item.price[3];
                                    break;
                                default:    // Não passou a quantidade de dias certo manda a soma de todos os preços que tem no iff
                                    _item.price = sIff.getInstance().SUM_ARRAY_PRICE_ULONG(item.price);
                                    break;
                            }

                            _item.STDA_C_ITEM_QNTD = 1;// item.Shop.flag_shop.time_shop.uc_time_start;
                            _item.flag_time = 2;
                            _item.flag = 0x20;
                            _item.STDA_C_ITEM_TIME = (ushort)((ushort)_bi.time * 24); // Horas

                        }
                        else if (_bi.time == 0 && !empty_price && _bi.qntd == 0)
                        {

                            message_pool.push(new message("[item_manager::initItemFromBuyItem][Error] Player[UID=" + (_pi.uid)
                                    + "] tentou inicializar Caddie Item[TYPEID=" + (_bi._typeid) + "] sem tempo e sem quantidade no jogo e no IFF_STRUCT ele tem tempo. Hacker ou Command GM.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                            _item._typeid = 0u;

                            return;
                        }


                    }
                    break;
                case IFF_GROUP.SET_ITEM:
                    {
                        var item = sIff.getInstance().findSetItem(_bi._typeid);

                        BEGIN_INIT_BUYITEM(_pi, ref _item, _bi, _gift_opt.IsTrue(), _chk_lvl.IsTrue(), item);
                    }
                    break;
                case IFF_GROUP.SKIN:     //falta
                    {
                        var item = sIff.getInstance().findSkin(_bi._typeid);

                        BEGIN_INIT_BUYITEM(_pi, ref _item, _bi, _gift_opt.IsTrue(), _chk_lvl.IsTrue(), item);

                        // ESSE AQUI É ONDE COMEÇA OS TEMPO, [1] É DO 1 A 365 DIAS, [7] É DO 7 A 365 DIAS
                        //item.Shop.flag_shop.time_shop.uc_time_start //---- AS SKINS É DO 7

                        // Aqui não precisa ver se tem time_limit e time_start, so tem que verificar se tem o item.price[0~4]
                        var empty_price = sIff.getInstance().EMPTY_ARRAY_PRICE(item.price);
                        if (_bi.time > 0 && !empty_price)
                        {

                            switch (_bi.time)
                            {   // Dias
                                case 1:
                                    if (_item.is_cash.IsTrue() ? _bi.cookie == item.price[0] : _bi.pang == item.price[0])
                                        _item.price = (uint)item.price[0];
                                    break;
                                case 7:
                                    if (_item.is_cash.IsTrue() ? _bi.cookie == item.price[1] : _bi.pang == item.price[1])
                                        _item.price = (uint)item.price[1];
                                    break;
                                case 15:
                                    if (_item.is_cash.IsTrue() ? _bi.cookie == item.price[2] : _bi.pang == item.price[2])
                                        _item.price = (uint)item.price[2];
                                    break;
                                case 30:
                                    if (_item.is_cash.IsTrue() ? _bi.cookie == item.price[3] : _bi.pang == item.price[3])
                                        _item.price = (uint)item.price[3];
                                    break;
                                case 365:
                                    if (_item.is_cash.IsTrue() ? _bi.cookie == item.price[4] : _bi.pang == item.price[4])
                                        _item.price = (uint)item.price[4];
                                    break;
                                default:    // Não passou a quantidade de dias certo manda a soma de todos os preços que tem no iff
                                    _item.price = (uint)sIff.getInstance().SUM_ARRAY_PRICE_ULONG(item.price);
                                    break;
                            }
                            _item.STDA_C_ITEM_QNTD = 1;
                            _item.flag_time = 4;
                            _item.flag = 0x20;
                            _item.STDA_C_ITEM_TIME = (ushort)_bi.time;  // Dias

                        }
                        else if (_bi.time > 0 && (item.Shop.flag_shop.time_shop.active || item.Shop.flag_shop.time_shop.dia > 0))
                        {

                            message_pool.push(new message("[item_manager::initItemFromBuyItem][WARNING] Player[UID=" + (_pi.uid)
                                     + "] inicializou Skin[TYPEID=" + (_bi._typeid) + "] com tempo no jogo e no IFF_STRUCT, mas ele nao tem os precos de tempo no IFF_STRUCT. Box ou Comando GM", type_msg.CL_FILE_LOG_AND_CONSOLE));

                            // Qntd tem que ser 1 por que o item é por tempo
                            if (_bi.qntd > 1)
                                _item.qntd = _bi.qntd = 1;

                            _item.STDA_C_ITEM_QNTD = 1;// item.Shop.flag_shop.time_shop.uc_time_start;
                            _item.flag_time = 4;
                            _item.flag = 0x40;
                            _item.STDA_C_ITEM_TIME = (ushort)_bi.time;  // Dias

                        }
                        else if (_bi.time == 0 && !empty_price && _bi.qntd > 0)
                        {

                            message_pool.push(new message("[item_manager::initItemFromBuyItem][WARNING] Player[UID=" + (_pi.uid)
                                     + "] tentou inicializar Skin[TYPEID=" + (_bi._typeid) + "] sem tempo no jogo e no IFF_STRUCT ele tem tempo. Hacker ou Command GM.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                            _bi.time = (short)(_bi.qntd > 365 ? 365 : _bi.qntd);

                            // Qntd tem que ser 1 por que o item é por tempo
                            if (_bi.qntd > 1)
                                _item.qntd = _bi.qntd = 1;

                            switch (_bi.time)
                            {   // Dias
                                case 1:
                                    if (_item.is_cash.IsTrue() ? _bi.cookie == item.price[0] : _bi.pang == item.price[0])
                                        _item.price = (uint)item.price[0];
                                    break;
                                case 7:
                                    if (_item.is_cash.IsTrue() ? _bi.cookie == item.price[1] : _bi.pang == item.price[1])
                                        _item.price = (uint)item.price[1];
                                    break;
                                case 15:
                                    if (_item.is_cash.IsTrue() ? _bi.cookie == item.price[2] : _bi.pang == item.price[2])
                                        _item.price = (uint)item.price[2];
                                    break;
                                case 30:
                                    if (_item.is_cash.IsTrue() ? _bi.cookie == item.price[3] : _bi.pang == item.price[3])
                                        _item.price = (uint)item.price[3];
                                    break;
                                case 365:
                                    if (_item.is_cash.IsTrue() ? _bi.cookie == item.price[4] : _bi.pang == item.price[4])
                                        _item.price = (uint)item.price[4];
                                    break;
                                default:    // Não passou a quantidade de dias certo manda a soma de todos os preços que tem no iff
                                    _item.price = (uint)sIff.getInstance().SUM_ARRAY_PRICE_ULONG(item.price);
                                    break;
                            }

                            _item.STDA_C_ITEM_QNTD = 1;
                            _item.flag_time = 4;
                            _item.flag = 0x20;
                            _item.STDA_C_ITEM_TIME = (ushort)_bi.time;  // Dias

                        }
                        else if (_bi.time == 0 && !empty_price && _bi.qntd == 0)
                        {

                            message_pool.push(new message("[item_manager::initItemFromBuyItem][Error] Player[UID=" + (_pi.uid)
                                    + "] tentou inicializar Skin[TYPEID=" + (_bi._typeid) + "] sem tempo e sem quantidade no jogo e no IFF_STRUCT ele tem tempo. Hacker ou Command GM.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                            _item._typeid = 0u;

                            return;
                        }



                    }
                    break;
                case IFF_GROUP.HAIR_STYLE:
                    {
                        var item = sIff.getInstance().findHairStyle(_bi._typeid);

                        BEGIN_INIT_BUYITEM(_pi, ref _item, _bi, _gift_opt.IsTrue(), _chk_lvl.IsTrue(), item);
                    }
                    break;
                case IFF_GROUP.MASCOT: //falta
                    {
                        var item = sIff.getInstance().findCaddieItem(_bi._typeid);

                        BEGIN_INIT_BUYITEM(_pi, ref _item, _bi, _gift_opt.IsTrue(), _chk_lvl.IsTrue(), item);

                        // Aqui não precisa ver se tem time_limit e time_start, so tem que verificar se tem o item.price[0~4]
                        var empty_price = sIff.getInstance().EMPTY_ARRAY_PRICE(item.price);

                        if (_bi.time > 0 && !empty_price)
                        {

                            switch (_bi.time)
                            {   // Dias
                                case 1:
                                    if (_item.is_cash.IsTrue() ? _bi.cookie == item.price[0] : _bi.pang == item.price[0])
                                        _item.price = (uint)item.price[0];
                                    break;
                                case 7:
                                    if (_item.is_cash.IsTrue() ? _bi.cookie == item.price[1] : _bi.pang == item.price[1])
                                        _item.price = (uint)item.price[1];
                                    break;
                                case 15:
                                    if (_item.is_cash.IsTrue() ? _bi.cookie == item.price[2] : _bi.pang == item.price[2])
                                        _item.price = (uint)item.price[2];
                                    break;
                                case 30:
                                    if (_item.is_cash.IsTrue() ? _bi.cookie == item.price[3] : _bi.pang == item.price[3])
                                        _item.price = (uint)item.price[3];
                                    break;
                            }

                            _item.STDA_C_ITEM_QNTD = 1;// item.Shop.flag_shop.time_shop.uc_time_start;
                            _item.flag_time = 2;
                            _item.flag = 0x20;
                            _item.STDA_C_ITEM_TIME = Convert.ToUInt16(_bi.time * 24);   // Horas

                        }
                        else if (_bi.time > 0 && (item.Shop.flag_shop.time_shop.active || item.Shop.flag_shop.time_shop.dia > 0))
                        {

                            message_pool.push(new message("[item_manager::initItemFromBuyItem][WARNING] Player[UID=" + (_pi.uid)
                                    + "] inicializou Caddie Item[TYPEID=" + (_bi._typeid) + "] com tempo no jogo e no IFF_STRUCT, mas ele nao tem os precos de tempo no IFF_STRUCT. Box ou Comando GM", type_msg.CL_FILE_LOG_AND_CONSOLE));

                            // Qntd tem que ser 1 por que o item é por tempo
                            if (_bi.qntd > 1)
                                _item.qntd = _bi.qntd = 1;

                            _item.STDA_C_ITEM_QNTD = 1;// item.Shop.flag_shop.time_shop.uc_time_start;
                            _item.flag_time = 4;
                            _item.flag = 0x40;
                            _item.STDA_C_ITEM_TIME = Convert.ToUInt16(_bi.time);    // Dias

                        }
                        else if (_bi.time == 0 && !empty_price && _bi.qntd > 0)
                        {

                            message_pool.push(new message("[item_manager::initItemFromBuyItem][WARNING] Player[UID=" + (_pi.uid)
                                    + "] tentou inicializar Caddie Item[TYPEID=" + (_bi._typeid) + "] sem tempo no jogo e no IFF_STRUCT ele tem tempo. Hacker ou Command GM.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                            _bi.time = Convert.ToInt16(_bi.qntd > 30 ? 30 : _bi.qntd);

                            // Qntd tem que ser 1 por que o item é por tempo
                            if (_bi.qntd > 1)
                                _item.qntd = _bi.qntd = 1;

                            switch (_bi.time)
                            {   // Dias
                                case 1:
                                    if (_item.is_cash.IsTrue() ? _bi.cookie == item.price[0] : _bi.pang == item.price[0])
                                        _item.price = (uint)item.price[0];
                                    break;
                                case 7:
                                    if (_item.is_cash.IsTrue() ? _bi.cookie == item.price[1] : _bi.pang == item.price[1])
                                        _item.price = (uint)item.price[1];
                                    break;
                                case 15:
                                    if (_item.is_cash.IsTrue() ? _bi.cookie == item.price[2] : _bi.pang == item.price[2])
                                        _item.price = (uint)item.price[2];
                                    break;
                                case 30:
                                    if (_item.is_cash.IsTrue() ? _bi.cookie == item.price[3] : _bi.pang == item.price[3])
                                        _item.price = (uint)item.price[3];
                                    break;
                                default:    // Não passou a quantidade de dias certo manda a soma de todos os preços que tem no iff
                                    _item.price = sIff.getInstance().SUM_ARRAY_PRICE_ULONG(item.price);
                                    break;
                            }

                            _item.STDA_C_ITEM_QNTD = 1;// item.Shop.flag_shop.time_shop.uc_time_start;
                            _item.flag_time = 2;
                            _item.flag = 0x20;
                            _item.STDA_C_ITEM_TIME = Convert.ToUInt16(_bi.time * 24);   // Horas

                        }
                        else if (_bi.time == 0 && !empty_price && _bi.qntd == 0)
                        {

                            message_pool.push(new message("[item_manager::initItemFromBuyItem][Error] Player[UID=" + (_pi.uid)
                                    + "] tentou inicializar Caddie Item[TYPEID=" + (_bi._typeid) + "] sem tempo e sem quantidade no jogo e no IFF_STRUCT ele tem tempo. Hacker ou Command GM.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                            _item._typeid = 0u;

                            return;
                        }

                    }
                    break;
                case IFF_GROUP.FURNITURE:
                    {
                        var item = sIff.getInstance().findFurniture(_bi._typeid);

                        BEGIN_INIT_BUYITEM(_pi, ref _item, _bi, _gift_opt.IsTrue(), _chk_lvl.IsTrue(), item);

                        if (_item.STDA_C_ITEM_QNTD == 0)
                            _item.STDA_C_ITEM_QNTD = (ushort)_item.qntd;

                    }
                    break;
                case IFF_GROUP.AUX_PART:
                    {
                        var item = sIff.getInstance().findAuxPart(_bi._typeid);

                        BEGIN_INIT_BUYITEM(_pi, ref _item, _bi, _gift_opt.IsTrue(), _chk_lvl.IsTrue(), item);

                        if (_item.STDA_C_ITEM_QNTD == 0)
                            _item.STDA_C_ITEM_QNTD = (ushort)_item.qntd;

                    }
                    break;
                case IFF_GROUP.CARD:
                    {
                        var item = sIff.getInstance().findCard(_bi._typeid);

                        BEGIN_INIT_BUYITEM(_pi, ref _item, _bi, _gift_opt.IsTrue(), _chk_lvl.IsTrue(), item);

                        if (_item.STDA_C_ITEM_QNTD <= 0)
                            _item.STDA_C_ITEM_QNTD = (ushort)_item.qntd;

                    }
                    break;
                default:
                    message_pool.push(new message("Player[UID=" + (_pi.uid) + "] Tentou comprar um item que nao tem no shop para vender. typeid: " + (_bi._typeid), type_msg.CL_FILE_LOG_AND_CONSOLE));
                    break;
            }
        }

        public static void initItemFromEmailItem(PlayerInfo _pi, ref stItem _item, EmailInfo.item _ei_item)
        {
            BuyItem item = new BuyItem
            {
                id = (int)_ei_item.id,
                _typeid = _ei_item._typeid,
                qntd = _ei_item.qntd,
                time = (short)(_ei_item.flag_time == 2/*Hora*/ ? _ei_item.tempo_qntd / 24 : _ei_item.tempo_qntd)
            };

            _item.flag_time = _ei_item.flag_time;

            // Aqui tem que criar o proprio dele por que tem o tipo do tempo[dias, horas, minutos, segundos] e etc
            initItemFromBuyItem(_pi, ref _item, item, false, 0, 0, 1/*não checar o level*/);
        }

        //// Check is have setitem in email
        public static void checkSetItemOnEmail(Player _session, EmailInfo _ei)
        {
            if (!_ei.itens.Any())
                throw new exception("[item_manager::checkSetItemOnEmail][Error] email not have item for check", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE._ITEM_MANAGER, 20, 0));

            for (var i = 0; i < _ei.itens.Count(); ++i)
            {
                if (isSetItem(_ei.itens[i]._typeid))
                {
                    var v_item = getItemOfSetItem(_session, _ei.itens[i]._typeid, false, 1/*Não verifica o Level*/);

                    if (!v_item.Any())
                    {
                        foreach (var el in v_item)
                            _ei.itens.Add(new EmailInfo.item(uint.MaxValue, el._typeid, el.flag_time, el.qntd, el.STDA_C_ITEM_TIME, 0, 0, 0/*flag GM*/, 0, "", 0));

                        _ei.itens.RemoveAt(i--);
                    }
                }
            }

        }

        public static RetAddItem.TYPE addItem(stItem _item, uint _uid, byte _gift_flag, byte _purchase, bool _dup = false)
        {
            return RetAddItem.TYPE.T_SUCCESS;
        }
        public static RetAddItem addItem(List<stItem> _v_item, uint _uid, byte _gift_flag, byte _purchase, bool _dup = false)
        {
            return new RetAddItem();
        }
        public static RetAddItem addItem(List<stItemEx> _v_item, uint _uid, byte _gift_flag, byte _purchase, bool _dup = false)
        {
            return new RetAddItem();
        }
        public static RetAddItem.TYPE addItem(stItem _item, Player _session, byte _gift_flag, byte _purchase, bool _dup = false)
        {
            return RetAddItem.TYPE.T_SUCCESS;
        } /*_dub pode duplicar*/
        public static RetAddItem addItem(List<stItem> _v_item, Player _session, byte _gift_flag, byte _purchase, bool _dup = false)
        {
            return new RetAddItem();
        }
        public static RetAddItem addItem(List<stItemEx> _v_item, Player _session, byte _gift_flag, byte _purchase, bool _dup = false)
        {
            return new RetAddItem();
        }
        public static RetAddItem addItem(Dictionary<uint, stItem> _v_item, Player _session, byte _gift_flag, byte _purchase, bool _dup = false)
        {
            return new RetAddItem();
        }
        public static RetAddItem addItem(Dictionary<uint, stItemEx> _v_item, Player _session, byte _gift_flag, byte _purchase, bool _dup = false)
        {
            return new RetAddItem();
        }

        //// Give Itens
        public static int giveItem(stItem _item, Player _session, byte _gift_flag) { return 1; }
        public static int giveItem(List<stItem> _v_item, Player _session, byte _gift_flag) { return 1; }
        public static int giveItem(List<stItemEx> _v_item, Player _session, byte _gift_flag) { return 1; }

        //// Remove Item
        public static int removeItem(stItem _item, Player _session)
        {
            if (!_session.getState())
            {
                throw new exception("[item_manager::removeItem][Error] session nao esta conectada.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE._ITEM_MANAGER,
                    0, 8));
            }

            int ret_id = -1;

            switch ((IFF_GROUP)sIff.getInstance().getItemGroupIdentify(_item._typeid))
            {
                case IFF_GROUP.AUX_PART: // Warehouse
                    {
                        var pWi = _session.m_pi.findWarehouseItemById((uint)_item.id);

                        if (pWi == null)
                        {
                            message_pool.push(new message("[item_manager::removeItem][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou remover um AuxPart[TYPEID=" + Convert.ToString(_item._typeid) + ", ID=" + Convert.ToString(_item.id) + "] que ele nao tem. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                            return -1;
                        }

                        if (pWi.STDA_C_ITEM_QNTD <= (short)_item.qntd)
                        { // Exclui o Item[AxuPart]

                            _item.stat.qntd_ant = (uint)pWi.STDA_C_ITEM_QNTD;

                            _item.STDA_C_ITEM_QNTD = (ushort)(pWi.STDA_C_ITEM_QNTD * -1);

                            pWi.STDA_C_ITEM_QNTD = 0;

                            _item.stat.qntd_dep = (uint)pWi.STDA_C_ITEM_QNTD;

                            //NormalManagerDB.add(0,
                            //    new CmdDeleteItem(_session.m_pi.uid, pWi.id),
                            //    item_manager.SQLDBResponse,
                            //    null);

                            var it = _session.m_pi.findWarehouseItemById(pWi.id);

                            if (it != null) // null)
                            {
                                _session.m_pi.mp_wi.Remove(it.id);
                            }

                            ret_id = _item.id;

                            // Se deletou a AuxPart que estava equipada 
                            // Desequipa o AuxPart
                            var v_ci = _session.isAuxPartEquiped(_item._typeid);

                            if (!v_ci.empty())
                            {

                                foreach (var el in v_ci)
                                {

                                    if (el != null)
                                    {

                                        // Desequipa o AuxPart
                                        el.unequipAuxPart(_item._typeid);

                                        // Update ON DB
                                        NormalManagerDB.add(0,
                                            new CmdUpdateCharacterAllPartEquiped(_session.m_pi.uid, el),
                                            item_manager.SQLDBResponse,
                                            null);

#if _DEBUG
							message_pool.push(new message("[item_manager::removeItem][Log] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] desequipou o AuxPart[TYPEID=" + Convert.ToString(_item._typeid) + "] do Character[TYPEID=" + Convert.ToString(el._typeid) + ", ID=" + Convert.ToString(el.id) + "] por que ele foi deletado.", type_msg.CL_FILE_LOG_AND_CONSOLE));
#endif // _DEBUG

                                        // Update ON GAME
                                        var p = new PangyaBinaryWriter((ushort)0x6B);

                                        p.WriteByte(4); // 4 Sucesso
                                        p.WriteByte(0); // Character All Parts

                                        p.WriteBytes(el.ToArray());

                                        packet_func.session_send(p,
                                            _session, 1);
                                    }
                                }
                            }
                        }
                        else
                        { // Att quantidade do Item

                            _item.stat.qntd_ant = (uint)pWi.STDA_C_ITEM_QNTD;

                            pWi.STDA_C_ITEM_QNTD -= (short)_item.qntd;

                            _item.stat.qntd_dep = (uint)pWi.STDA_C_ITEM_QNTD;

                            //NormalManagerDB.add(0,
                            //    new CmdUpdateItemQntd(_session.m_pi.uid,
                            //        pWi.id, pWi.STDA_C_ITEM_QNTD),
                            //    item_manager.SQLDBResponse,
                            //    null);

                            ret_id = (int)pWi.id;
                        }

                        break;
                    }
                case IFF_GROUP.BALL: // Warehouse
                    {
                        var pWi = _session.m_pi.findWarehouseItemById((uint)_item.id);

                        if (pWi == null)
                        {
                            message_pool.push(new message("[item_manager::removeItem][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou remover um Ball[TYPEID=" + Convert.ToString(_item._typeid) + ", ID=" + Convert.ToString(_item.id) + "] que ele nao tem. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                            return -1;
                        }

                        if (pWi.STDA_C_ITEM_QNTD <= (short)_item.qntd)
                        { // Exclui o Item[Ball]

                            _item.stat.qntd_ant = (uint)pWi.STDA_C_ITEM_QNTD;

                            _item.STDA_C_ITEM_QNTD = (ushort)(pWi.STDA_C_ITEM_QNTD * -1);

                            pWi.STDA_C_ITEM_QNTD = 0;

                            _item.stat.qntd_dep =(uint) pWi.STDA_C_ITEM_QNTD;

                            // Passa o typeid do Warehouse para o _item para garantir, se não tiver colocado o typeid, na estrutura
                            _item._typeid = pWi._typeid;

                            //NormalManagerDB.add(0,
                            //    new CmdDeleteBall(_session.m_pi.uid, pWi.id),
                            //    item_manager.SQLDBResponse,
                            //    null);

                            //auto it = VECTOR_FIND_ITEM(_session.m_pi.v_wi, id, == , pWi->id);
                            var it = _session.m_pi.findWarehouseItemById(pWi.id);

                            if (it != null) // _session.m_pi.mp_wi.end())
                            {
                                _session.m_pi.mp_wi.Remove(it.id);
                            }

                            ret_id = _item.id;

                            // Se deletou a bola que estava equipada 
                            // troca para a bola padrão
                            if (_session.m_pi.ue.ball_typeid == _item._typeid)
                            {

                                WarehouseItemEx pBall = null;
                                var p = new PangyaBinaryWriter();

                                if ((pBall = _session.m_pi.findWarehouseItemByTypeid(0x14000000)) == null)
                                {
                                    message_pool.push(new message("[item_manager::removeItem][Error][WARNING] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao tem a Comet padrao para substituir a bola[TYPEID=" + Convert.ToString(_item._typeid) + "] deletada. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                }
                                else
                                { // Substitui

                                    // Update ON SERVER
                                    _session.m_pi.ue.ball_typeid = DEFAULT_COMET_TYPEID; // Comet Padrão

                                    _session.m_pi.ei.comet = pBall;

                                    // Update ON DB
                                    NormalManagerDB.add(0,
                                        new CmdUpdateBallEquiped(_session.m_pi.uid, _session.m_pi.ue.ball_typeid),
                                        item_manager.SQLDBResponse,
                                        null);

#if _DEBUG
						message_pool.push(new message("[item_manager::removeItem][Log] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] substitui a bola[TYPEID=" + Convert.ToString(_item._typeid) + "] deletada pela COMET PADRAO[TYPEID=" + Convert.ToString(DEFAULT_COMET_TYPEID) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
#endif // _DEBUG

                                    // Update ON GAME
                                   
                                    packet_func.session_send(packet_func.pacote04B(
                                        _session, 2),
                                        _session, 1);
                                }
                            }

                        }
                        else
                        { // Att quantidade do Item

                            _item.stat.qntd_ant = (uint)pWi.STDA_C_ITEM_QNTD;

                            pWi.STDA_C_ITEM_QNTD -= (short)_item.qntd;

                            _item.stat.qntd_dep =(uint) pWi.STDA_C_ITEM_QNTD;

                            NormalManagerDB.add(0,
                                new CmdUpdateBallQntd(_session.m_pi.uid,
                                    (int)pWi.id, (uint)pWi.STDA_C_ITEM_QNTD),
                                item_manager.SQLDBResponse,
                                null);

                            ret_id = (int)pWi.id;

                        }

                        break;
                    }
                case IFF_GROUP.CADDIE:
                    {
                        var pCi = _session.m_pi.findCaddieById((uint)_item.id);

                        if (pCi == null)
                        {
                            message_pool.push(new message("[item_manager::removeItem][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou remover um Caddie[TYPEID=" + Convert.ToString(_item._typeid) + ", ID=" + Convert.ToString(_item.id) + "] que ele nao tem. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                            return -1;
                        }

                        _item.stat.qntd_ant = 1;

                        _item.STDA_C_ITEM_QNTD = ushort.MaxValue;

                        _item.stat.qntd_dep = 0;

                        //NormalManagerDB.add(0,
                        //    new CmdDeleteCaddie(_session.m_pi.uid, pCi.id),
                        //    item_manager.SQLDBResponse,
                        //    null);

                        //auto it = VECTOR_FIND_ITEM(_session.m_pi.v_ci, second.id, == , pCi->id);
                        var it = _session.m_pi.findCaddieById(pCi.id);

                        if (it != null) // _session.m_pi.mp_ci.end())
                        {
                            _session.m_pi.mp_ci.Remove(it.id);
                        }

                        ret_id = _item.id;
                        break;
                    }
                case IFF_GROUP.CAD_ITEM:
                    // por hora nao exclui esse por que ainda nao vi nenhum que exclui caddie item
                    message_pool.push(new message("[item_manager::removeItem][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou remover um caddie item[TYPEID=" + Convert.ToString(_item._typeid) + ", ID=" + Convert.ToString(_item.id) + "] mas nao e permitido. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    break;
                case IFF_GROUP.CARD:
                    {
                        var pCi = _session.m_pi.findCardById((uint)_item.id);

                        if (pCi == null)
                        {
                            message_pool.push(new message("[item_manager::removeItem][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou remover um Card[TYPEID=" + Convert.ToString(_item._typeid) + ", ID=" + Convert.ToString(_item.id) + "] que ele nao tem. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                            return -1;
                        }

                        if (pCi.qntd <= (int)_item.qntd)
                        { // Exclui Item[Card]


                            _item.stat.qntd_ant = pCi.qntd;

                            _item.stat.qntd_dep = 0;

                            _item.STDA_C_ITEM_QNTD = (ushort)((short)pCi.qntd * -1);

                            pCi.qntd = 0;

                            //NormalManagerDB.add(0,
                            //    new CmdDeleteCard(_session.m_pi.uid, pCi.id),
                            //    item_manager.SQLDBResponse,
                            //    null);

                            //auto it = VECTOR_FIND_ITEM(_session.m_pi.v_card_info, id, == , pCi->id);
                            var it = _session.m_pi.findCardById(pCi.id);

                            if (it != null) // _session.m_pi.v_card_info.end())
                            {
                                _session.m_pi.v_card_info.Remove(it.id);
                            }

                            ret_id = _item.id;

                        }
                        else
                        { // Att quantidade do Item

                            _item.stat.qntd_ant = pCi.qntd;

                            pCi.qntd -= _item.qntd;

                            _item.stat.qntd_dep = pCi.qntd;

                            NormalManagerDB.add(0,
                                new CmdUpdateCardQntd(_session.m_pi.uid,
                                    (int)pCi.id, pCi.qntd),
                                item_manager.SQLDBResponse,
                                null);

                            ret_id = (int)pCi.id;
                        }

                        break;
                    }
                case IFF_GROUP.CHARACTER:
                    message_pool.push(new message("[item_manager::removeItem][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou remover um character[TYPEID=" + Convert.ToString(_item._typeid) + ", ID=" + Convert.ToString(_item.id) + "] mas nao e permitido. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    break;
                case IFF_GROUP.CLUBSET: // Warehouse
                    {
                        var pWi = _session.m_pi.findWarehouseItemById((uint)_item.id);

                        if (pWi == null)
                        {
                            message_pool.push(new message("[item_manager::removeItem][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou remover um ClubSet[TYPEID=" + Convert.ToString(_item._typeid) + ", ID=" + Convert.ToString(_item.id) + "] que ele nao tem. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                            return -1;
                        }

                        _item.stat.qntd_ant = 1;

                        _item.STDA_C_ITEM_QNTD = ushort.MaxValue;

                        _item.stat.qntd_dep = 0;

                        //NormalManagerDB.add(0,
                        //    new CmdDeleteItem(_session.m_pi.uid, pWi.id),
                        //    item_manager.SQLDBResponse,
                        //    null);

                        //auto it = VECTOR_FIND_ITEM(_session.m_pi.v_wi, id, == , pWi->id);
                        var it = _session.m_pi.findWarehouseItemById(pWi.id);

                        if (it != null) // _session.m_pi.mp_wi.end())
                        {
                            _session.m_pi.mp_wi.Remove(it.id);
                        }

                        ret_id = _item.id;

                        break;
                    }
                case IFF_GROUP.FURNITURE:
                    {
                        var pFi = _session.m_pi.findMyRoomItemById((uint)_item.id);

                        if (pFi == null)
                        {
                            message_pool.push(new message("[item_manager::removeItem][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou remover um Furniture[TYPEID=" + Convert.ToString(_item._typeid) + ", ID=" + Convert.ToString(_item.id) + "] que ele nao tem. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                            return -1;
                        }

                        _item.stat.qntd_ant = 1;

                        _item.STDA_C_ITEM_QNTD = ushort.MaxValue;

                        _item.stat.qntd_dep = 0;

                        //NormalManagerDB.add(0,
                        //    new CmdDeleteFurniture(_session.m_pi.uid, pFi.id),
                        //    item_manager.SQLDBResponse,
                        //    null);

                        //auto it = VECTOR_FIND_ITEM(_session.m_pi.v_mri, id, == , pFi->id);
                        var it = _session.m_pi.findMyRoomItemById(pFi.id);

                        if (it != null) // _session.m_pi.v_mri.end())
                        {
                            _session.m_pi.v_mri.Remove(it);
                        }

                        ret_id = _item.id;
                        break;
                    }
                case IFF_GROUP.HAIR_STYLE:
                    message_pool.push(new message("[item_manager::removeItem][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou remover um hairstyle[TYPEID=" + Convert.ToString(_item._typeid) + ", ID=" + Convert.ToString(_item.id) + "] mas nao e permitido. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    break;
                case IFF_GROUP.ITEM: // Warehouse
                    {
                        var pWi = _session.m_pi.findWarehouseItemById((uint)_item.id);

                        if (pWi == null)
                        {
                            message_pool.push(new message("[item_manager::removeItem][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou remover um Item[TYPEID=" + Convert.ToString(_item._typeid) + ", ID=" + Convert.ToString(_item.id) + "] que ele nao tem. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                            return -1;
                        }

                        if (pWi.STDA_C_ITEM_QNTD <= (short)_item.qntd)
                        { // Exclui o Item[Item]

                            _item.stat.qntd_ant = (uint)pWi.STDA_C_ITEM_QNTD;

                            _item.STDA_C_ITEM_QNTD = (ushort)(pWi.STDA_C_ITEM_QNTD * -1);

                            pWi.STDA_C_ITEM_QNTD = 0;

                            _item.stat.qntd_dep =(uint) pWi.STDA_C_ITEM_QNTD;

                            //NormalManagerDB.add(0,
                            //    new CmdDeleteItem(_session.m_pi.uid, pWi.id),
                            //    item_manager.SQLDBResponse,
                            //    null);

                            //auto it = VECTOR_FIND_ITEM(_session.m_pi.v_wi, id, == , pWi->id);
                            var it = _session.m_pi.findWarehouseItemById(pWi.id);

                            if (it != null) // _session.m_pi.mp_wi.end())
                            {
                                _session.m_pi.mp_wi.Remove(it.id);
                            }

                            ret_id = _item.id;

                        }
                        else
                        { // Att quantidade do Item

                            _item.stat.qntd_ant = (uint)pWi.STDA_C_ITEM_QNTD;

                            pWi.STDA_C_ITEM_QNTD -= (short)_item.qntd;

                            _item.stat.qntd_dep =(uint) pWi.STDA_C_ITEM_QNTD;

                            //NormalManagerDB.add(0,
                            //    new CmdUpdateItemQntd(_session.m_pi.uid,
                            //        pWi.id, pWi.STDA_C_ITEM_QNTD),
                            //    item_manager.SQLDBResponse,
                            //    null);

                            ret_id = (int)pWi.id;
                        }

                        break;
                    }
                case IFF_GROUP.MASCOT:
                    {
                        var pMi = _session.m_pi.findMascotById((uint)_item.id);

                        if (pMi == null)
                        {
                            message_pool.push(new message("[item_manager::removeItem][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou remover um Mascot[TYPEID=" + Convert.ToString(_item._typeid) + ", ID=" + Convert.ToString(_item.id) + "] que ele nao tem. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                            return -1;
                        }

                        _item.stat.qntd_ant = 1;

                        _item.STDA_C_ITEM_QNTD = ushort.MaxValue;

                        _item.stat.qntd_dep = 0;

                        //NormalManagerDB.add(0,
                        //    new CmdDeleteMascot(_session.m_pi.uid, pMi.id),
                        //    item_manager.SQLDBResponse,
                        //    null);

                        //auto it = VECTOR_FIND_ITEM(_session.m_pi.v_mi, id, == , pMi->id);
                        var it = _session.m_pi.findMascotByTypeid(pMi.id);

                        if (it != null) // _session.m_pi.mp_mi.end())
                        {
                            _session.m_pi.mp_mi.Remove(it.id);
                        }

                        ret_id = _item.id;

                        break;
                    }
                case IFF_GROUP.PART: // Warehouse
                    {
                        var pWi = _session.m_pi.findWarehouseItemById((uint)_item.id);

                        if (pWi == null)
                        {
                            message_pool.push(new message("[item_manager::removeItem][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou remover um Part[TYPEID=" + Convert.ToString(_item._typeid) + ", ID=" + Convert.ToString(_item.id) + "] que ele nao tem. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                            return -1;
                        }

                        _item.stat.qntd_ant = 1;

                        _item.STDA_C_ITEM_QNTD = ushort.MaxValue;

                        _item.stat.qntd_dep = 0;

                        //NormalManagerDB.add(0,
                        //    new CmdDeleteItem(_session.m_pi.uid, pWi.id),
                        //    item_manager.SQLDBResponse,
                        //    null);

                        //auto it = VECTOR_FIND_ITEM(_session.m_pi.v_wi, id, == , pWi->id);
                        var it = _session.m_pi.findWarehouseItemById(pWi.id);

                        if (it != null) // _session.m_pi.mp_wi.end())
                        {
                            _session.m_pi.mp_wi.Remove(it.id);
                        }

                        ret_id = _item.id;

                        // Se deletou a Part que estava equipada 
                        // Desequipa o Part
                        var ci = _session.isPartEquiped(_item._typeid);

                        if (ci != null)
                        {

                            // Desequipa o Part
                            ci.unequipPart(_item._typeid);

                            // Update ON DB
                            NormalManagerDB.add(0,
                                new CmdUpdateCharacterAllPartEquiped(_session.m_pi.uid, ci),
                                item_manager.SQLDBResponse,
                                null);

#if _DEBUG
				message_pool.push(new message("[item_manager::removeItem][Log] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] desequipou o Part[TYPEID=" + Convert.ToString(_item._typeid) + "] por que ele foi deletado.", type_msg.CL_FILE_LOG_AND_CONSOLE));
#endif // _DEBUG

                            // Update ON GAME
                            var p = new PangyaBinaryWriter((ushort)0x6B);

                            p.WriteByte(4); // 4 Sucesso
                            p.WriteByte(0); // Character All Parts

                            p.WriteBytes(ci.ToArray());

                            packet_func.session_send(p,
                                _session, 1);
                        }

                        break;
                    }
                case IFF_GROUP.SET_ITEM:
                    message_pool.push(new message("[item_manager::removeItem][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou remover um SetItem[TYPEID=" + Convert.ToString(_item._typeid) + ", ID=" + Convert.ToString(_item.id) + "] mas nao e permitido. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    break;
                case IFF_GROUP.SKIN: // Warehouse
                    {
                        var pWi = _session.m_pi.findWarehouseItemById((uint)_item.id);

                        if (pWi == null)
                        {
                            message_pool.push(new message("[item_manager::removeItem][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou remover um Skin[TYPEID=" + Convert.ToString(_item._typeid) + ", ID=" + Convert.ToString(_item.id) + "] que ele nao tem. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                            return -1;
                        }

                        _item.stat.qntd_ant = 1;

                        _item.STDA_C_ITEM_QNTD = ushort.MaxValue;

                        _item.stat.qntd_dep = 0;

                        //NormalManagerDB.add(0,
                        //    new CmdDeleteItem(_session.m_pi.uid, pWi.id),
                        //    item_manager.SQLDBResponse,
                        //    null);

                        //auto it = VECTOR_FIND_ITEM(_session.m_pi.v_wi, id, == , pWi->id);
                        var it = _session.m_pi.findWarehouseItemById(pWi.id);

                        if (it != null) // _session.m_pi.mp_wi.end())
                        {
                            _session.m_pi.mp_wi.Remove(it.id);
                        }

                        ret_id = _item.id;

                        break;
                    }
                default:
                    break;
            }

            return ret_id;
        }


        public static int removeItem(List<stItem> _v_item, Player _session)
    {

        int i;

        for (i = 0; i < _v_item.Count(); ++i)
        {
            if (removeItem(_v_item[i], _session) <= 0)
            {
                _v_item.RemoveAt(i--);
			}
		}
            return 1;
                    }
    public static int removeItem(List<stItemEx> _v_item, Player _session) { return 1; }

        //// Transfer Item [Personal Shop]
        ////public static WarehouseItemEx* transferItem(Player _s_snd, Player _s_rcv, PersonalShopItem _psi, PersonalShopItem _psi_r);
        public static WarehouseItemEx transferItem(Player _s_snd, Player _s_rcv, PersonalShopItem _psi, PersonalShopItem _psi_r)
        { return new WarehouseItemEx(); }

        //// CadieMagicBox Exchange Check
        public static int exchangeCadieMagicBox(Player _session, uint _typeid, int _id, uint _qntd)
        { return 1; }

        //// Tiki Shop Excgange Item Check
        public static List<stItem> exchangeTikiShop(Player _session, uint _typeid, int _id, uint _qntd)
        { return new List<stItem>(); }

        //// Open Ticket Report Scroll
        public static void openTicketReportScroll(Player _session, int _ticket_scroll_item_id, int _ticket_scroll_id, bool _upt_on_game = false) { }

        //// Verifies
        public static bool isSetItem(uint _typeid)
        {
            return sIff.getInstance()._getItemGroupIdentify(_typeid) == IFF_GROUP.SET_ITEM;
        }
        // Métodos de verificação de tempo, sobrecarga com diferentes tipos de entrada
        public static bool isTimeItem(stItem.stDate _date)
        {
            // Implementação aqui
            return true; // Exemplo
        }

        public static bool isTimeItem(stItem.stDate.stDateSys _date)
        {
            // Implementação aqui
            return true; // Exemplo
        }
        // Métodos que verificam se o usuário possui o item
        public static bool ownerItem(uint _uid, uint _typeid)
        {
            // Implementação aqui
            return true; // Exemplo
        }

        public static bool ownerSetItem(uint _uid, uint _typeid)
        {
            // Implementação aqui
            return true; // Exemplo
        }

        public static bool ownerCaddieItem(uint _uid, uint _typeid)
        {
            // Implementação aqui
            return true; // Exemplo
        }

        public static bool ownerHairStyle(uint _uid, uint _typeid)
        {


            var hair = sIff.getInstance().findHairStyle(_typeid);

            if (hair != null)
            {
                var character = _ownerHairStyle(_uid, _typeid);

                if (!(character.id > 0)) // Não tem o Character
                {
                    return true;
                }

                if (character.default_hair == hair.Color)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool ownerMailBoxItem(uint _uid, uint _typeid)
        {
            // Implementação aqui
            return true; // Exemplo
        }
        // Métodos que retornam informações sobre o item proprietário
        public static CaddieInfoEx _ownerCaddieItem(uint _uid, uint _typeid)
        {
            // Implementação aqui
            return new CaddieInfoEx(); // Exemplo
        }

        public static CharacterInfo _ownerHairStyle(uint _uid, uint _typeid)
        {
            // Implementação aqui
            return new CharacterInfo(); // Exemplo
        }

        public static MascotInfoEx _ownerMascot(uint _uid, uint _typeid)
        {
            // Implementação aqui
            return new MascotInfoEx(); // Exemplo
        }

        public static WarehouseItemEx _ownerBall(uint _uid, uint _typeid)
        {
            // Implementação aqui
            return new WarehouseItemEx(); // Exemplo
        }

        public static CardInfo _ownerCard(uint _uid, uint _typeid)
        {
            // Implementação aqui
            return new CardInfo(); // Exemplo
        }

        public static WarehouseItemEx _ownerAuxPart(uint _uid, uint _typeid)
        {
            // Implementação aqui
            return new WarehouseItemEx(); // Exemplo
        }

        public static WarehouseItemEx _ownerItem(uint _uid, uint _typeid)
        {
            // Implementação aqui
            return new WarehouseItemEx(); // Exemplo
        }

        public static TrofelEspecialInfo _ownerTrofelEspecial(uint _uid, uint _typeid)
        { 
            var type_trofel = sIff.getInstance().getItemSubGroupIdentify24(_typeid);

            var type = Cmd.CmdFindTrofelEspecial.eTYPE.ESPECIAL;

            if (type_trofel == 1 || type_trofel == 2)
            {
                type = CmdFindTrofelEspecial.eTYPE.ESPECIAL;
            }
            else if (type_trofel == 3)
            {
                type = CmdFindTrofelEspecial.eTYPE.GRAND_PRIX;
            }

            CmdFindTrofelEspecial cmd_fts = new CmdFindTrofelEspecial(_uid, // Waiter
                _typeid, type);

            NormalManagerDB.add(0,
                cmd_fts, null, null);

            if (cmd_fts.getException().getCodeError() != 0)
            {
                throw cmd_fts.getException();
            }

            return cmd_fts.getInfo();
        }
        // Métodos de verificação de tempo
        public static bool betweenTimeSystem(stItem.stDate _date)
        {
            // Implementação aqui
            return true; // Exemplo
        }

        public static bool betweenTimeSystem(PangyaTime _date)
        {
            // Implementação aqui
            return true; // Exemplo
        }

        public static bool betweenTimeSystem(stItem.stDate.stDateSys _date)
        {
            // Implementação aqui
            return true; // Exemplo
        }

        protected static void SQLDBResponse(int _msg_id,
            Pangya_DB _pangya_db,
            object _arg)
        {

            if (_arg == null)
            {
                message_pool.push(new message("[item_manager::SQLDBResponse][WARNING] _arg is null na msg_id = " + Convert.ToString(_msg_id), type_msg.CL_FILE_LOG_AND_CONSOLE));
                return;
            }

            // Por Hora só sai, depois faço outro tipo de tratamento se precisar
            if (_pangya_db.getException().getCodeError() != 0)
            {
                message_pool.push(new message("[item_manager::SQLDBResponse][Error] " + _pangya_db.getException().getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                return;
            }

            // isso aqui depois pode mudar para o Item_manager, que vou tirar de ser uma classe static e usar ela como objeto(instancia)
            //var _session = reinterpret_cast< Player* >(_arg);

            switch (_msg_id)
            {
                case 1: // Add Character
                    {
                        // Não usa mais
                        break;
                    }
                case 2: // add Caddie
                    {
                        // Não usa mais
                        break;
                    }
                case 3: // add Warehouse
                    {
                        // Não usa mais
                        break;
                    }
                case 4: // Add HairStyle
                    {
                        // Esse aqui só tem att o cabelo no character do Player
                        break;
                    }
                case 5: // Caddie Item
                    {
                        // Esse aqui só att o parts typeid do caddie e o tempo no DB, não precisa esperar a resposta do DB já que ele não retorna nada
                        break;
                    }
                case 6: // Update Mascot Time
                    {
                        // Não usa mais
                        break;
                    }
                case 7: // Update Ball Quantidade
                    {
                        // Não usa mais
                        break;
                    }
                case 8: // Update Card Quantidade
                    {
                        // Não usa mais
                        break;
                    }
                case 9: // Update Item Quantidade
                    {
                        // Não usa mais
                        break;
                    }
                case 10: // Delete Item
                    {
                        // Não usa mais
                        break;
                    }
                case 11: // Delete Ball
                    {
                        // Não usa mais
                        break;
                    }
                case 12: // Delete Card
                    {
                        // Não usa mais
                        break;
                    }
                case 13: // Gift ClubSet
                    {
                        // Não usa mais
                        break;
                    }
                case 14: // Gift Part
                    {
                        // Não usa mais
                        break;
                    }
                case 15: // Personal Shop Log
                    {
                        // Não usa por que não retorna nada é um INSERT
                        break;
                    }
                case 16: // Personal Shop Transfer Part
                    {
                        // Não usa por que não retorna nada é um UPDATE
                        break;
                    }
                case 17: // Update Character Equipped
                    {
                        // Não usa por que não retorna nada é um UPDATE
                        break;
                    }
                case 18: // Update Trofel Especial e Grand Prix
                    {
                        // Não usa por que não retorna nada é um UPDATE
                        break;
                    }
                case 19: // Update Premium Ticket Time
                    {
                        // Não usa por que não retorna nada é um UPDATE
                        break;
                    }
                case 20: // Update Clubset Time
                    {
                        // Não uda por que não retorna nada é um UPDATE
                        break;
                    }
                case 0:
                default:
                    break;
            }
        }
    }
}