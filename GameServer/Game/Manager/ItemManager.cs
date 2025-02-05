using GameServer.PangType;
using GameServer.Session;
using PangyaAPI.Network.Pangya_St;
using PangyaAPI.SQL;
using PangyaAPI.Utilities.Log;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.Game.Manager
{
    /// <summary>
    /// Manipulation Add, Check, Remove Items
    /// </summary>
    public class ItemManager
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

            public RetAddItem(uint _ul = 0u)
            {
                clear();
            }
            public void Dispose()
            {
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
        public ItemManager()
        {
        }

        // Gets
        public static List<stItem> getItemOfSetItem(Player _session,
            uint _typeid, bool _shop,
            int _chk_level)
        {
            return new List<stItem>();
        }

        // Init Item From Buy --Gift opt-- Item
        public static void initItemFromBuyItem(PlayerInfo _pi,
            stItem _item, BuyItem _bi,
            bool _shop, int _option,
            int _gift_opt = 0,
            int _chk_lvl = 0)
        {

        }

        public static void initItemFromEmailItem(PlayerInfo _pi,
                stItem _item,
                EmailInfo.item _ei_item)
        {

        }

        // Check is have setitem in email
        public static void checkSetItemOnEmail(Player _session, EmailInfo _ei)
        {
        }

        // Add Itens
        public static RetAddItem.TYPE addItem(stItem _item,
            uint _uid, byte _gift_flag,
            byte _purchase,
            bool _dup = false)
        {
            return RetAddItem.TYPE.TR_SUCCESS_PANG_AND_EXP_AND_CP_POUCH_WITH_ERROR;
        }

        public static RetAddItem addItem(List<stItem> _v_item, uint _uid, byte _gift_flag, byte _purchase, bool _dup = false)
        {
            return new RetAddItem();
        }

        public static RetAddItem addItem(List<stItemEx> _v_item, uint _uid, byte _gift_flag, byte _purchase, bool _dup = false)
        {
            return new RetAddItem();
        }

        public static RetAddItem.TYPE addItem(stItem _item,
                Player _session,
                byte _gift_flag,
                byte _purchase,
                bool _dup = false)
        {
            return RetAddItem.TYPE.TR_SUCCESS_PANG_AND_EXP_AND_CP_POUCH_WITH_ERROR;
        }

        public static RetAddItem addItem(List<stItem> _v_item, Player _session, byte _gift_flag, byte _purchase, bool _dup = false)
        {
            return new RetAddItem();
        }

        public static RetAddItem addItem(List<stItemEx> _v_item, Player _session, byte _gift_flag, byte _purchase, bool _dup = false)
        {
            return new RetAddItem();
        }

        public static RetAddItem addItem(SortedDictionary<uint, stItem> _v_item, Player _session, byte _gift_flag, byte _purchase, bool _dup = false)
        {
            return new RetAddItem();
        }

        public static RetAddItem addItem(SortedDictionary<uint, stItemEx> _v_item,
                Player _session,
                byte _gift_flag,
                byte _purchase,
                bool _dup = false)

        {
            return new RetAddItem();
        }

        // Give Itens
        public static uint giveItem(stItem _item,
            Player _session,
            byte _gift_flag)
        {
            return 0;
        }

        public static uint giveItem(List<stItem> _v_item,
                Player _session,
                byte _gift_flag)
        {
            return 0;
        }

        public static uint giveItem(List<stItemEx> _v_item,
                Player _session,
                byte _gift_flag)
        {
            return 0;
        }
        // Remove Item
        public static uint removeItem(stItem _item, Player _session)
        {
            return 0;
        }

        public static uint removeItem(List<stItem> _v_item, Player _session)
        {
            return 0;
        }
        public static uint removeItem(List<stItemEx> _v_item, Player _session)
        {
            return 0;
        }

        // Transfer Item [Personal Shop]
        //static WarehouseItemEx* transferItem(Player& _s_snd, Player& _s_rcv, PersonalShopItem& _psi, PersonalShopItem& _psi_r);

        //WarehouseItemEx* item_manager::transferItem(Player& _s_snd, Player& _s_rcv, PersonalShopItem& _psi, PersonalShopItem& _psi_r) {
        public static WarehouseItemEx transferItem(Player _s_snd,
            Player _s_rcv,
            PersonalShopItem _psi,
            PersonalShopItem _psi_r)
        {
            WarehouseItemEx ret_wi = null;

            return ret_wi;
        }

        // CadieMagicBox Exchange Check
        public static uint exchangeCadieMagicBox(Player _session,
            uint _typeid, uint _id,
            uint _qntd)
        {
            return 0;
        }

        // Tiki Shop Excgange Item Check
        public static List<stItem> exchangeTikiShop(Player _session,
            uint _typeid, uint _id,
            uint _qntd)
        {
            return new List<stItem>();
        }

        // Open Ticket Report Scroll
        public static void openTicketReportScroll(Player _session,
            uint _ticket_scroll_item_id,
            uint _ticket_scroll_id,
            bool _upt_on_game = false)
        {

        }

        // Verifies
        public static bool isSetItem(uint _typeid)
        {
            return true;
        }

        public static bool isTimeItem(stItem.stDate _date)
        {
            return _date.active == 1 || _date.date.sysDate[0].Year != 0 || _date.date.sysDate[1].Year != 0;
        }

        public static bool isTimeItem(stItem.stDate.stDateSys _date)
        {
            return (_date.sysDate[0].Year != 0 || _date.sysDate[1].Year != 0);
        }

        // Owner All Item(ns) "ownerItem"
        public static bool ownerItem(uint _uid, uint _typeid)
        {
            bool ret = false;

            return ret;
        }

        public static bool ownerSetItem(uint _uid, uint _typeid)
        {
            return false;
        }
        public static bool ownerCaddieItem(uint _uid, uint _typeid)
        {
            return false;
        }
        public static bool ownerHairStyle(uint _uid, uint _typeid)
        {
            return false;
        }

        public static bool ownerMailBoxItem(uint _uid, uint _typeid)
        {
               
            return false;
        }

        // Suporte Owner Find
        public static CaddieInfoEx _ownerCaddieItem(uint _uid, uint _typeid)
        {
            return new CaddieInfoEx();
        }

        public static CharacterInfo _ownerHairStyle(uint _uid, uint _typeid)
        {
                 

            return new CharacterInfo(); //CharacterInfo Vazio
        }

        public static MascotInfoEx _ownerMascot(uint _uid, uint _typeid)
        {
            return new MascotInfoEx();
        }

        public static WarehouseItemEx _ownerBall(uint _uid, uint _typeid)
        {
            return new WarehouseItemEx();
        }

        public static CardInfo _ownerCard(uint _uid, uint _typeid)
        {
            return new CardInfo();
        }

        public static WarehouseItemEx _ownerAuxPart(uint _uid, uint _typeid)
        {                             
            return new WarehouseItemEx();
        }

        public static WarehouseItemEx _ownerItem(uint _uid, uint _typeid)
        {
            return new WarehouseItemEx();
        }

        public static TrofelEspecialInfo _ownerTrofelEspecial(uint _uid, uint _typeid)
        {
            return null;
        }

        //public static bool betweenTimeSystem(stItem.stDate _date)
        //{

        //    if (!isTimeItem(_date))
        //    {
        //        throw new exception("[item_manager::betweenTimeSystem][Error] Item nao e um item de tempo.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE._ITEM_MANAGER,
        //            3, 0));
        //    }

        //    FILETIME ft1 = new FILETIME();
        //    FILETIME ft2 = new FILETIME();
        //    FILETIME ft3 = new FILETIME();

        //    PangyaTime st = new PangyaTime();

        //    st.CreateTime();                       

        //    SystemTimeToFileTime(_date.date.sysDate[0], ft1);
        //    SystemTimeToFileTime(_date.date.sysDate[1], ft2);
        //    SystemTimeToFileTime(st, ft3);

        //    // Date de termino é 0, então passa a do LocalTime, por que ele só tem a data de quando começa
        //    if (ft2.dwHighDateTime == 0 && ft2.dwLowDateTime == 0)
        //    {                      
        //        ft2 = ft3;
        //    }
                                                 
        //    // C++ TO C# CONVERTER TASK: There is no equivalent to 'reinterpret_cast' in C#:
        //    return (reinterpret_cast<ULARGE_INTEGER>(ft1).QuadPart <= reinterpret_cast<ULARGE_INTEGER>(ft3).QuadPart && reinterpret_cast<ULARGE_INTEGER>(ft3).QuadPart <= reinterpret_cast<ULARGE_INTEGER>(ft2).QuadPart);
        //}

        //public static bool betweenTimeSystem(IFF.DateDados _date)
        //{
        //    return betweenTimeSystem((stItem.stDate)_date);
        //}

        //public static bool betweenTimeSystem(stItem.stDate.stDateSys _date)
        //{                                               
        //    stItem.stDate date = new stItem.stDate() { date = _date };

        //    return betweenTimeSystem(date);
        //}

        protected static void SQLDBResponse(uint _msg_id,
            Pangya_DB _pangya_db,
            object _arg)
        {

            if (_arg == null)
            {
                message_pool.push(new message("[item_manager::SQLDBResponse][WARNING] _arg is nullptr na msg_id = " + Convert.ToString(_msg_id), type_msg.CL_FILE_LOG_AND_CONSOLE));
                return;
            }

            // Por Hora só sai, depois faço outro tipo de tratamento se precisar
            if (_pangya_db.getException().getCodeError() != 0)
            {
                message_pool.push(new message("[item_manager::SQLDBResponse][Error] " + _pangya_db.getException().getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                return;
            }

            // isso aqui depois pode mudar para o Item_manager, que vou tirar de ser uma classe static e usar ela como objeto(instancia)
            //auto _session = reinterpret_cast< Player* >(_arg);

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