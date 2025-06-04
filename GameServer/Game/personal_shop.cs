using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Pangya_GameServer.GameType;
using Pangya_GameServer.PacketFunc;
using Pangya_GameServer.Session;
using PangyaAPI.IFF.JP.Extensions;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using static Pangya_GameServer.GameType._Define;
namespace Pangya_GameServer.Game
{
    public class PersonalShop
    {
      
        public enum STATE : uint
        {
            OPEN_EDIT,
            OPEN
        }

        public class Locker
        {
            public Locker(PersonalShop _shop)
            {
                this.m_shop = _shop; 
            }
             
            protected PersonalShop m_shop;
        }

        public PersonalShop(Player _session)
        {
            lock (_lockObj)
            { 
                this.m_owner = _session;
                this.m_name = "";
                this.m_visit_count = 0u;
                this.m_pang_sale = 0Ul;
                this.m_state = STATE.OPEN_EDIT;
            }
        }

        ~PersonalShop()
        {
            destroy();
            clearItem();
        }

        // Gets
        public string getName()
        { 
            return m_name;
        }

        public uint getVisitCount()
        { 
            return m_visit_count;
        }

        public ulong getPangSale()
        { 
            return m_pang_sale;
        }

        public Player getOwner()
        {
            lock (_lockObj)
            {
                return m_owner;
            }
        }

        public STATE getState()
        {
            lock (_lockObj)
            {
                return m_state;
            }
        }

        public uint getCountItem()
        {
            lock (_lockObj)
            { 
                return (uint)v_item.Count();
            }
        }

        public List<Player> getClients()
        { 
            return v_open_shop_visit;
        }

        // Sets
        public void setName(string _name)
        { 
            if (_name.Length == 0)
            {
                throw new exception("[PersonalShop::setName][Error] _name is empty", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP,
                    1, 0));
            }

            m_name = _name;
        }

        public void setState(STATE _state)
        { 
            m_state = _state;
        }

        public void clearItem()
        { 
            if (v_item.Any())
            {
                v_item.Clear();
            }
        }

        public void pushItem(PersonalShopItem _psi)
        { 
            // Verifica aqui se esse item por ser colocar no shop

            if (_psi.item._typeid == 0)
            {
                throw new exception("[PersonalShop::pushItem][Error] player[UID=" + Convert.ToString(m_owner.m_pi.uid) + "] tentou colocar um invalid item no Personal Shop dele. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP,
                    7, 0));
            }

            var @base = sIff.getInstance().findCommomItem(_psi.item._typeid);

            if (@base == null)
            {
                throw new exception("[PersonalShop::pushItem][Error] player[UID=" + Convert.ToString(m_owner.m_pi.uid) + "] tentou colocar um item[TYPEID=" + Convert.ToString(_psi.item._typeid) + "] que nao existe no IFF_STRUCT do server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP,
                    8, 0));
            }

            if (!@base.Shop.flag_shop.can_send_mail_and_personal_shop)
            {
                throw new exception("[PersonalShop::pushItem][Error] player[UID=" + Convert.ToString(m_owner.m_pi.uid) + "] tentou colocar um item[TYPEID=" + Convert.ToString(_psi.item._typeid) + "] que nao pode ser vendido no Personal Shop. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP,
                    9, 0));
            }

            // Verifica o pre�o do item
            if (_psi.item.pang < ITEM_MIN_PRICE || _psi.item.pang > ITEM_MAX_PRICE)
            {
                throw new exception("[PersonalShop::pushItem][Error] player[UID=" + Convert.ToString(m_owner.m_pi.uid) + "] tentou colocar um item[TYPEID=" + Convert.ToString(_psi.item._typeid) + ", Price=" + Convert.ToString(_psi.item.pang) + "] que o preco esta fora do limite[MIN=" + Convert.ToString(ITEM_MIN_PRICE) + ", MAX=" + Convert.ToString(ITEM_MAX_PRICE) + "]. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP, 24, 0));
            }

            // Card pre�o controle
            if (sIff.getInstance().getItemGroupIdentify(_psi.item._typeid) == sIff.getInstance().CARD)
            {

                var card = sIff.getInstance().findCard(_psi.item._typeid);

                if (card == null)
                {
                    throw new exception("[PersonalShop::pushItem][Error] player[UID=" + Convert.ToString(m_owner.m_pi.uid) + "] tentou colocar um card[TYPEID=" + Convert.ToString(_psi.item._typeid) + "] que nao existe no IFF_STRUCT do server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP,
                        21, 0));
                }

                switch (card.Rarity)
                {
                    case 0: // Normal
                        if (_psi.item.pang > CARD_NORMAL_LIMIT_PRICE)
                        {
                            throw new exception("[PersonalShop::pushItem][Error] player[UID=" + Convert.ToString(m_owner.m_pi.uid) + "] tentou colocar um card[TYPEID=" + Convert.ToString(_psi.item._typeid) + ", TYPE=Normal, Price=" + Convert.ToString(_psi.item.pang) + "] que o preco passa do limite(" + Convert.ToString(CARD_NORMAL_LIMIT_PRICE) + "). Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP,
                                23, 0));
                        }
                        break;
                    case 1: // Rare
                        if (_psi.item.pang > CARD_RARE_LIMIT_PRICE)
                        {
                            throw new exception("[PersonalShop::pushItem][Error] player[UID=" + Convert.ToString(m_owner.m_pi.uid) + "] tentou colocar um card[TYPEID=" + Convert.ToString(_psi.item._typeid) + ", TYPE=Rare, Price=" + Convert.ToString(_psi.item.pang) + "] que o preco passa do limite(" + Convert.ToString(CARD_RARE_LIMIT_PRICE) + "). Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP,
                                23, 0));
                        }
                        break;
                    case 2: // Super Rare
                        if (_psi.item.pang > CARD_SUPER_RARE_LIMIT_PRICE)
                        {
                            throw new exception("[PersonalShop::pushItem][Error] player[UID=" + Convert.ToString(m_owner.m_pi.uid) + "] tentou colocar um card[TYPEID=" + Convert.ToString(_psi.item._typeid) + ", TYPE=Super Rare, Price=" + Convert.ToString(_psi.item.pang) + "] que o preco passa do limite(" + Convert.ToString(CARD_SUPER_RARE_LIMIT_PRICE) + "). Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP,
                                23, 0));
                        }
                        break;
                    case 3: // Secret
                        if (_psi.item.pang > CARD_SECRET_LIMIT_PRICE)
                        {
                            throw new exception("[PersonalShop::pushItem][Error] player[UID=" + Convert.ToString(m_owner.m_pi.uid) + "] tentou colocar um card[TYPEID=" + Convert.ToString(_psi.item._typeid) + ", TYPE=Secret, Price=" + Convert.ToString(_psi.item.pang) + "] que o preco passa do limite(" + Convert.ToString(CARD_SECRET_LIMIT_PRICE) + "). Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP,
                                23, 0));
                        }
                        break;
                    default: // Unknown Type
                        throw new exception("[PersonalShop::pushItem][Error] player[UID=" + Convert.ToString(m_owner.m_pi.uid) + "] tentou colocar um card[TYPEID=" + Convert.ToString(_psi.item._typeid) + ", TYPE=" + Convert.ToString((ushort)card.Rarity) + "] que o tipo eh desconhecido. (N,R,SR e SC) Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP, 22, 0));
                }
            }

            v_item.Add(_psi);
        }

        public void deleteItem(PersonalShopItem _psi)
        {

           // Locker _locker = new Locker(this);

            var item = findItemIndexById((int)_psi.item.id);

            if (v_item[item].item.id == _psi.item.id)
            {
                v_item.RemoveAt(item);
            }
            else
            {
                for (int ii = 0; ii < v_item.Count; ii++)
                {
                    if (v_item[ii].item.id == _psi.item.id)
                    {
                        v_item.RemoveAt(ii);
                        break; // Importante para evitar problemas após remover um item
                    }
                }
            }

        }

        public void putItemOnPacket(PangyaBinaryWriter _p)
        {

           // Locker _locker = new Locker(this);

            if (v_item.Count() == 0)
            {
                throw new exception("[PersonalShop::putItemOnPacket][Error] size vector item shop is zero", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP,
                    2, 0));
            }

            _p.WriteUInt32((uint)v_item.Count());

            for (var i = 0; i < v_item.Count(); ++i)
            {
                _p.WriteBytes(v_item[i].ToArray());
            }
        }

        // Find
        public PersonalShopItem findItemById(int _id)
        {

            if (_id <= 0)
            {
                throw new exception("[PersonalShop::findItemById][Error] _id[value=" + Convert.ToString(_id) + "] is invalid", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP,
                    3, 0));
            }

           // Locker _locker = new Locker(this);

            PersonalShopItem psi = null;

            foreach (var el in v_item)
            {
                if (el.item.id == _id)
                {
                    psi = el;
                    break;
                }
            }

            return psi;
        }

        public PersonalShopItem findItemByIndex(uint _index)
        {

           // Locker _locker = new Locker(this);

            PersonalShopItem psi = null;

            foreach (var el in v_item)
            {
                if (el.index == _index)
                {
                    psi = el;
                    break;
                }
            }

            return psi;
        }

        public int findItemIndexById(int _id)
        {

           // Locker _locker = new Locker(this);

            int index = -1;

            for (var i = 0; i < v_item.Count(); ++i)
            {
                if (v_item[i].item.id == _id)
                {
                    index = (int)i;
                    break;
                }
            }

            return (index);
        }

        public Player findClientByUID(uint _uid)
        {

           // Locker _locker = new Locker(this);

            Player client = null;

            foreach (var el in v_open_shop_visit)
            {
                if (el.m_pi.uid == _uid)
                {
                    client = el;
                    break;
                }
            }

            return client;
        }

        public int findClientIndexByUID(uint _uid)
        {

           // Locker _locker = new Locker(this);

            int index = -1;

            for (var i = 0; i < v_open_shop_visit.Count(); ++i)
            {
                if (v_open_shop_visit[i].m_pi.uid == _uid)
                {
                    index = (int)i;
                    break;
                }
            }

            return index;
        }

        // Visit
        public void addClient(Player _session)
        {

           // Locker _locker = new Locker(this);

            if (m_state != STATE.OPEN)
            {
                throw new exception("[PersonalShop::addClient][Error] client[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou entrar no shop do player[UID=" + Convert.ToString(m_owner.m_pi.uid) + "], mas ele nao esta aberto no momento. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP,
                    25, 0));
            }

            var client = findClientByUID(_session.m_pi.uid);

            if (client != null)
            {
                throw new exception("[PersonalShop::addClient][Error] client[UID=" + Convert.ToString(_session.m_pi.uid) + "] ja existe no Personal Shop do player[UID=" + Convert.ToString(m_owner.m_pi.uid) + "]. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP,
                    4, 0));
            }

            if (v_open_shop_visit.Count() >= LIMIT_VISIT_ON_SAME_TIME)
            {
                throw new exception("[PersonalShop::addClient][Log] client[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao pode entrar no shop por que ja chegou ao limit de clientes ao mesmo tempo no Personal Shop do player[UID=" + Convert.ToString(m_owner.m_pi.uid) + "]", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP,
                    6, 0));
            }

            v_open_shop_visit.Add(_session);

            // Add Contador de visitas
            m_visit_count++;
        }

        public void deleteClient(Player _session)
        {

           // Locker _locker = new Locker(this);

            var client = findClientIndexByUID(_session.m_pi.uid);

            if (client == -1)
            {
                throw new exception("[PersonalShop::deleteClient][Error] client[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao existe no vector de clientes do Personal Shop do player[UID=" + Convert.ToString(m_owner.m_pi.uid) + "]. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP,
                    5, 0));
            }

            if (v_open_shop_visit[client].m_pi.uid == _session.m_pi.uid)
            {
                v_open_shop_visit.RemoveAt(client);
            }
            else
            {
                for (int ii = 0; ii < v_open_shop_visit.Count; ii++)
                {
                    if (v_open_shop_visit[ii].m_pi.uid == _session.m_pi.uid)
                    {
                        v_open_shop_visit.RemoveAt(ii);
                        break; // Para evitar problemas de indexação após a remoção
                    }
                }
            }
        }

        public void buyItem(Player _session, PersonalShopItem _psi)
        {

           // Locker _locker = new Locker(this);

            if (m_state != STATE.OPEN)
            {
                throw new exception("[PersonalShop::buyItem][Error] client[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou comprar no shop do player[UID=" + Convert.ToString(m_owner.m_pi.uid) + "], mas ele nao esta aberto no momento. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP,
                    25, 0));
            }

            if (findClientByUID(_session.m_pi.uid) == null)
            {
                throw new exception("[PersonalShop::buyItem][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou comprar item[TYPEID=" + Convert.ToString(_psi.item._typeid) + ", ID=" + Convert.ToString(_psi.item.id) + "] no Shop[Owner UID=" + Convert.ToString(m_owner.m_pi.uid) + "], mas ele nao esta no shop do player. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP,
                    10, 0));
            }

            var psi_owner = findItemById((int)_psi.item.id);

            if (psi_owner == null || psi_owner.index != _psi.index)
            {
                throw new exception("[PersonalShop::buyItem][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou comprar item[TYPEID=" + Convert.ToString(_psi.item._typeid) + ", ID=" + Convert.ToString(_psi.item.id) + "] que nao tem no Shop[Owner UID=" + Convert.ToString(m_owner.m_pi.uid) + "]. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP,
                    11, 0));
            }

            if (psi_owner.item._typeid == 0)
            {
                throw new exception("[PersonalShop::buyItem][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou comprar item[TYPEID=" + Convert.ToString(psi_owner.item._typeid) + ", ID=" + Convert.ToString(psi_owner.item.id) + "] invalido no Shop[Owner UID=" + Convert.ToString(m_owner.m_pi.uid) + "]. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP,
                    12, 0));
            }

            var @base = sIff.getInstance().findCommomItem(psi_owner.item._typeid);

            if (@base == null)
            {
                throw new exception("[PersonalShop::buyItem][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou comprar item[TYPEID=" + Convert.ToString(psi_owner.item._typeid) + ", ID=" + Convert.ToString(psi_owner.item.id) + "] invalido que nao tem no IFF_STRUCT do server, no Shop[Owner UID=" + Convert.ToString(m_owner.m_pi.uid) + "]. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP,
                    13, 0));
            }

            if (!@base.Shop.flag_shop.can_send_mail_and_personal_shop)
            {
                throw new exception("[PersonalShop::buyItem][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou comprar item[TYPEID=" + Convert.ToString(psi_owner.item._typeid) + ", ID=" + Convert.ToString(psi_owner.item.id) + "] que nao pode ser vendido no Personal Shop, no Shop[Owner UID=" + Convert.ToString(m_owner.m_pi.uid) + "]. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP,
                    14, 0));
            }

            if (_session.m_pi.ui.pang < (psi_owner.item.pang * _psi.item.qntd))
            {
                throw new exception("[PersonalShop::buyItem][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou comprar item[TYPEID=" + Convert.ToString(psi_owner.item._typeid) + ", ID=" + Convert.ToString(psi_owner.item.id) + "] mas ele nao tem pangs suficiente, no Shop[Owner UID=" + Convert.ToString(m_owner.m_pi.uid) + "]. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP,
                    15, 0));
            }

            if (!@base.Level.GoodLevel((byte)_session.m_pi.level))
            {
                throw new exception("[PersonalShop::buyItem][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou comprar item[TYPEID=" + Convert.ToString(psi_owner.item._typeid) + ", ID=" + Convert.ToString(psi_owner.item.id) + "] mas ele nao tem level suficiente, no Shop[Owner UID=" + Convert.ToString(m_owner.m_pi.uid) + "]. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP,
                    16, 0));
            }

            if (_psi.item.qntd > psi_owner.item.qntd)
            {
                throw new exception("[PersonalShop::buyItem][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou comprar item[TYPEID=" + Convert.ToString(psi_owner.item._typeid) + ", ID=" + Convert.ToString(psi_owner.item.id) + ", QNTD=" + Convert.ToString(psi_owner.item.qntd) + "] mas ele quer comprar uma quantidade(" + Convert.ToString(_psi.item.qntd) + ") do item maior do que esta a venda. ", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP,
                    26, 0));
            }

            //// Depois o Personal Shop vai poder vender card, ent�o tem que procurar nos card tbm

            //ulong pang = psi_owner.item.pang * _psi.item.qntd;

            //// c�pia do personalshopitem, para enviar para o Player que comprou que muda o id, quando compra card ou item, part e clubset ele so transfere msm,
            //// mas pode ser que no pangya original ele cria um novo e s� transferi os dados
            //var psi_r = _psi;

            ////WarehouseItemEx* pWi = nullptr;
            //object pWi = null;

            //if ((pWi = item_manager.transferItem(m_owner,
            //    _session, _psi, psi_r)) != null)
            //{
            //    if (psi_owner.item.qntd == _psi.item.qntd)
            //    {
            //        deleteItem(_psi);
            //    }
            //    else
            //    {

            //        psi_owner.item.qntd -= _psi.item.qntd;

            //    }

            //    // ATUALIZA OS PANGS DO PLAYER NO DB, CHAMNANDO AS FUN��ES QUE J� CRIEI, MAS POR HORA S� VOU MEXER NO SERVER
            //    _session.m_pi.consomePang(pang);

            //    // Tira os 5% dos pangs
            //    m_owner.m_pi.addPang((pang = (ulong)Math.Round(pang * 0.95f)));

            //    // Pang(s) do Personal Shop
            //    m_pang_sale += pang;

            //    // Att no Jogo
            //    packet p = new packet();

            //    // Card
            //    if (sIff.getInstance().getItemGroupIdentify(_psi.item._typeid) == sIff.getInstance().CARD)
            //    {


            //        var ci_r = (CardInfo)(pWi);

            //        // Tira de quem vendeu
            //        p.init_plain((ushort)0xEC);

            //        p.WriteUInt32(1); // OK

            //        p.WriteByte(1); // 1 Tira Item

            //        p.WriteUInt64(pang);

            //        p.WriteBytes(_psi.Build());

            //        p.WriteByte(5);

            //        // Att id por que esta o novo item do Player que vai receber o item
            //        ci_r.id = _psi.item.id;
            //        ci_r.qntd = _psi.item.qntd;

            //        p.WriteBytes(ci_r.Build());

            //        packet_func.session_send(p,
            //            m_owner, 1);

            //        // Esse Pacote � o de add ele soma, n�o atualiza do zero como o que tira do player
            //        ci_r.id = psi_r.item.id;
            //        ci_r.qntd = psi_r.item.qntd;

            //        // Add para quem comprou, aqui � s� para mostrar a tela que comprou e liberar o player
            //        p.init_plain((ushort)0xEC);

            //        p.WriteUInt32(1); // OK

            //        p.WriteByte(0); // 0 Add Item

            //        p.WriteUInt64(_session.m_pi.ui.pang);

            //        p.WriteBytes(psi_r.Build());

            //        p.WriteByte(5);

            //        p.WriteBytes(ci_r.Build());

            //        packet_func.session_send(p,
            //            _session, 1);

            //    }
            //    else
            //    { // WarehouseItem


            //        var wi_r = (WarehouseItemEx)(pWi);

            //        // Tira de quem vendeu
            //        p.init_plain((ushort)0xEC);

            //        p.WriteUInt32(1); // OK

            //        p.WriteByte(1); // 1 Tira Item

            //        p.WriteUInt64(pang);

            //        p.WriteBytes(_psi.Build());

            //        p.WriteByte((sIff.getInstance().getItemGroupIdentify(_psi.item._typeid) == sIff.getInstance().ITEM) ? 1 : 3);

            //        // Att id por que esta o novo item do Player que vai receber o item
            //        wi_r.id = _psi.item.id;
            //        wi_r.c[0] = (short)_psi.item.qntd;

            //        p.WriteBytes(wi_r.Build());

            //        packet_func.session_send(p,
            //            m_owner, 1);

            //        // Esse Pacote � o de add ele soma, n�o atualiza do zero como o que tira do player
            //        wi_r.id = psi_r.item.id;
            //        wi_r.c[0] = (short)psi_r.item.qntd;

            //        // Add para quem comprou
            //        p.init_plain((ushort)0xEC);

            //        p.WriteUInt32(1); // OK

            //        p.WriteByte(0); // 0 Add Item

            //        p.WriteUInt64(_session.m_pi.ui.pang);

            //        p.WriteBytes(psi_r.Build());

            //        p.WriteByte((sIff.getInstance().getItemGroupIdentify(psi_r.item._typeid) == sIff.getInstance().ITEM) ? 1 : 3);

            //        p.WriteBytes(wi_r.Build());

            //        packet_func.session_send(p,
            //            _session, 1);
            //    }

            //    // Atualiza no Shop os Itens que foram comprado
            //    p.init_plain((ushort)0xED);

            //    p.WritePStr(m_owner.m_pi.nickname);

            //    p.WriteUInt32(m_owner.m_pi.uid);

            //    p.WriteBytes(_psi.Build());

            //    p.WriteInt32(v_item.Count() == 0 ? 3 : 1);

            //    shop_broadcast(p,
            //        _session, 1);

            //    //// Update Achievement ON SERVER, DB and GAME
            //    //SysAchievement sys_achieve = new SysAchievement();

            //    //sys_achieve.incrementCounter(0x6C400083u);

            //    //sys_achieve.finish_and_update(_session);

            //}
            //else
            //{
            //    throw new exception("[PersonalShop::buyItem][Error] nao conseguiu transferir o item[TYPEID=" + Convert.ToString(_psi.item._typeid) + ", ID=" + Convert.ToString(_psi.item.id) + "] da venda do personal shop, do player[UID=" + Convert.ToString(m_owner.m_pi.uid) + "] para o player[UID=" + Convert.ToString(_session.m_pi.uid) + "]", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP,
            //        19, 0));
            //}
        }
         
        private void destroy()
        {

           // Locker _locker = new Locker(this);

            if (v_open_shop_visit.Any())
            {
                v_open_shop_visit.Clear();
            }
        }

        public void shop_broadcast(PangyaBinaryWriter _p,
            Player _s, byte _debug)
        {

            if (_s == null)
            {
                throw new exception("[PersonalShop::shop_broadcast][Error] Session *_s is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP,
                    20, 0));
            }

           // Locker _locker = new Locker(this);

            var clients = v_open_shop_visit;

            // Envia para o dono do Personal Shop
            packet_func.session_send(_p,
                m_owner, 1);

            foreach (var el in clients)
            {
                packet_func.session_send(_p,
                    el, _debug);
            }
        }

        public void shop_broadcast(List<PangyaBinaryWriter> _v_p,
            Player _s, byte _debug)
        {

            if (_s == null)
            {
                throw new exception("[PersonalShop::shop_broadcast][Error] Session *_s is nullptr", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP,
                    20, 0));
            }

           // Locker _locker = new Locker(this);

            var clients = v_open_shop_visit;

            // Envia para o dono do Personal Shop
            packet_func.session_send(_v_p,
                m_owner, 1);

            foreach (var el in clients)
            {
                packet_func.session_send(_v_p,
                    el, _debug);
            }
        }

        protected string m_name = ""; // Nome da Loja
        protected Player m_owner; // Dono da Loja

        protected STATE m_state = new STATE(); // estado da loja, aberta ou editando

        protected uint m_visit_count = new uint(); // N�mero de visitantes que visitaram a loja

        protected ulong m_pang_sale = new ulong(); // pangs em caixa

        protected List<PersonalShopItem> v_item = new List<PersonalShopItem>(); // Itens da Loja   
        protected List<Player> v_open_shop_visit = new List<Player>(); // Os visitantes que est�o com o shop aberto
        private readonly object _lockObj = new object();
    }
}
