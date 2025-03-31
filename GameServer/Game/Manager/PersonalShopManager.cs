using GameServer.GameType;
using PangyaAPI.Utilities;
using System.Collections.Generic;
using System.Linq;
using System;
using _smp = PangyaAPI.Utilities.Log;
using GameServer.Session;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;
using GameServer.PacketFunc;
using PangyaAPI.Network.PangyaPacket;
namespace GameServer.Game.Manager
{
    public partial class PersonalShopManager
    {
        public enum eTYPE_LOCK : byte
        {
            TL_NONE,
            TL_SELECT,
            TL_DELETE
        }

        public partial class Locker
        {
            public Locker(PersonalShopManager _manager,
                uint _owner_uid,
                eTYPE_LOCK _type)
            {
                this.m_manager = _manager;
                this.m_owner_uid = _owner_uid;
                this.m_type = _type;

                m_manager.spinUp(m_owner_uid, m_type);
            }

            public void Dispose()
            {

                m_manager.spinDown(m_owner_uid, m_type);
            }

            protected PersonalShopManager m_manager;
            protected uint m_owner_uid = new uint();
            protected eTYPE_LOCK m_type = new eTYPE_LOCK();
        }

        public partial class PersonalShopCtx
        {
            public PersonalShopCtx(PersonalShop _shop,
                eTYPE_LOCK _type,
                int _count)
            {
                this.m_shop = _shop;
                this.m_type = _type;
                this.m_count = _count;
            }

            public PersonalShop m_shop;
            public eTYPE_LOCK m_type = new eTYPE_LOCK();
            public int m_count = new int();
        }


        public PersonalShopManager(RoomInfoEx _ri)
        {
            this.m_ri = _ri;
            this.mapShop = new Dictionary<Player, PersonalShopCtx>();
        }

        public void Dispose()
        {

            destroy();
        }

        public void destroy()
        {

            clear_shops();
        }

        public bool hasNameInSomeShop(string _name, uint _owner_uid)
        {

            bool ret = false;

            try
            {

                // lock
                @lock();

                ret = _hasNameInSomeShop(_name, _owner_uid);

                // unlock
                unlock();

            }
            catch (exception e)
            {

                // unlock
                unlock();

                _smp.message_pool.push(new message("[PersonalShopManager::hasNameInSomeShop][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return ret;
        }

        public PersonalShop findShop(Player _session)
        {

            PersonalShop ret = null;

            try
            {

                // lock
                @lock();

                ret = _findShop(_session);

                // unlock
                unlock();

            }
            catch (exception e)
            {

                // unlock
                unlock();

                _smp.message_pool.push(new message("[PersonalShopManager::findShop][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return ret;
        }

        public PersonalShop findShop(uint _owner_uid)
        {

            PersonalShop ret = null;

            try
            {

                // lock
                @lock();

                ret = _findShop(_owner_uid);

                // unlock
                unlock();

            }
            catch (exception e)
            {

                // unlock
                unlock();

                _smp.message_pool.push(new message("[PersonalShopManager::findShop][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return ret;
        }

        public Dictionary<Player, PersonalShopCtx> findShopIt(Player _session)
        {
            Dictionary<Player, PersonalShopCtx> ret = new Dictionary<Player, PersonalShopCtx>();

            try
            {
                // Lock para garantir segurança em ambiente multithread
                lock (this)
                {
                    ret = _findShopIt(_session);
                }
            }
            catch (Exception e)
            {
                // Registra o erro no sistema de mensagens
                _smp.message_pool.push(new message(
                    "[PersonalShopManager::findShopIt][ErrorSystem] " + e.Message,
                    type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return ret;
        }


        public Dictionary<Player, PersonalShopCtx> findShopIt(uint _owner_uid)
        {

            Dictionary<Player, PersonalShopCtx> ret = new Dictionary<Player, PersonalShopCtx>();

            try
            {

                // lock
                @lock();

                ret = _findShopIt(_owner_uid);

                // unlock
                unlock();

            }
            catch (exception e)
            {

                // unlock
                unlock();

                _smp.message_pool.push(new message("[PersonalShopManager::findShopIt][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return ret;
        }

        public void delete_shop(Player _session)
        {

            try
            {

                // lock
                @lock();

                _delete_shop(_session);

                // unlock
                unlock();

            }
            catch (exception e)
            {

                // unlock
                unlock();

                _smp.message_pool.push(new message("[PersonalShopManager::delete_shop][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public bool isItemForSale(Player _session, int _item_id)
        {

            Locker _locker = new Locker(this,
                _session.m_pi.uid,
                eTYPE_LOCK.TL_SELECT);

            try
            {

                // Sala não é Lounge, não tem como o player abrir um shop
                if ((RoomInfo.TIPO)m_ri.tipo != RoomInfo.TIPO.LOUNGE)
                {
                    return false;
                }

                var ps = findShop(_session);

                // Player não tem um shop aberto na sala
                if (ps == null)
                {
                    return false;
                }

                // O item não está à venda no shop do player
                if (ps.findItemById(_item_id) == null)
                {
                    return false;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[PersonalShopManager::isItemForSale][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            // O item está à venda no shop do player
            return true;
        }

        public PlayerRoomInfo.PersonShop getPersonShop(Player _session)
        {

            Locker _locker = new Locker(this,
                _session.m_pi.uid,
                eTYPE_LOCK.TL_SELECT);

            PlayerRoomInfo.PersonShop person = new PlayerRoomInfo.PersonShop() { active = 0u };

            do
            {

                if ((RoomInfo.TIPO)m_ri.tipo != RoomInfo.TIPO.LOUNGE)
                {
                    break;
                }

                var ps = findShop(_session);

                if (ps == null)
                {
                    break;
                }

                if (ps.getState() == PersonalShop.STATE.OPEN_EDIT)
                {
                    break;
                }

                person.active = 1u;
                person.name =  ps.getName();

            } while (false);

            // C++ TO C# CONVERTER TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
            // ORIGINAL LINE: return person;
            return person;
        }

        public void destroyShop(Player _session)
        {

            Locker _locker = new Locker(this,
                _session.m_pi.uid,
                eTYPE_LOCK.TL_DELETE);

            try
            {

                delete_shop(_session);

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[PersonalShopManager::destroyShop][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public bool openShopToEdit(Player _session, ref PangyaBinaryWriter _out_packet)
        {

            Locker _locker = new Locker(this,
                _session.m_pi.uid,
                eTYPE_LOCK.TL_SELECT);

            PangyaBinaryWriter p = new PangyaBinaryWriter();

            try
            {

                if ((RoomInfo.TIPO)m_ri.tipo != RoomInfo.TIPO.LOUNGE)
                {
                    throw new exception("[PersonalShopManager::openShopToEdit][Error][WARNING] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou abri um personal shop para venda em uma sala[TIPO=" + Convert.ToString((ushort)(RoomInfo.TIPO)m_ri.tipo) + ", NUMERO=" + Convert.ToString(m_ri.numero) + "] diferente de Lounge. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP_MANAGER,
                        100, 52001001));
                }

                if (_session.m_pi.block_flag.m_flag.personal_shop)
                {
                    throw new exception("[PersonalShopManager::openShopToEdit][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou abrir um personal shop para vender na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas ele nao pode. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP_MANAGER,
                        4, 0x790001));
                }

                // Verifica o level do player e bloquea se não tiver level Beginner E
                if (_session.m_pi.level < (short)PangyaEnums.enLEVEL.BEGINNER_E)
                {
                    throw new exception("[PersonalShopManager::openShopToEdit][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + ", LEVEL=" + Convert.ToString(_session.m_pi.level) + "] tentou abrir um personal shop para vender na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas o level dele eh menor que Beginner E.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP_MANAGER,
                        3, 0));
                }

                if (mapShop.Count >= (uint)(m_ri.max_Player * 0.8f))
                {
                    throw new exception("[PersonalShopManager::openShopToEdit][Log] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] chegou no limite de shop(s) permitidos na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "]", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP_MANAGER,
                        101, 5200102));
                }

                PersonalShop ps = null;

                // Esse aqui não é para da esse erro por que o pacote que pede para editar a loja, é esse também
                if ((ps = findShop(_session)) != null)
                {

                    ps.setState(PersonalShop.STATE.OPEN_EDIT);

                    _smp.message_pool.push(new message("[PersonalShopManager::openShopToEdit][Log] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] Editando Personal Shop", type_msg.CL_FILE_LOG_AND_CONSOLE));

                }
                else
                {

                    // lock
                    @lock();
                    var r = new PersonalShopCtx(new PersonalShop(_session), eTYPE_LOCK.TL_SELECT, 1);
                    // Cria um Personal Shop para o player, por que ele não tem 1
                    mapShop.Add(_session, r);
                     if (r != null)
                    {

                        // unlock
                        unlock();

                        // Error, fail to insert personal shop into map
                        throw new exception("[PersonalShopManager::openShopToEdit][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu adicionar o shop do player para ao map.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP_MANAGER,
                            701, 5200701));
                    }

                    // new personal shop
                    ps = r.m_shop;

                    // unlock
                    unlock();
                }

                // Log
                _smp.message_pool.push(new message("[PersonalShopManager::openShopToEdit][Log] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] abriu Personal Shop[Owner UID=" + Convert.ToString(ps.getOwner().m_pi.uid) + ", STATE=" + Convert.ToString(ps.getState()) + ", Name=" + ps.getName() + ", Item Count=" + Convert.ToString(ps.getCountItem()) + ", Pang Sale=" + Convert.ToString(ps.getPangSale()) + "] para editar na sala[numero=" + Convert.ToString(m_ri.numero) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Sucesso
                _out_packet.init_plain((ushort)0xE5);

                _out_packet.WriteUInt32(1);

                _out_packet.WritePStr(_session.m_pi.nickname);
                _out_packet.WriteUInt32(_session.m_pi.uid);

                // sucesso, enviar o pacote para a sala toda
                return true;

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[PersonalShopManager::openShopToEdit][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain((ushort)0xE5);

                p.WriteUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.PERSONAL_SHOP_MANAGER) ? ExceptionError.STDA_SYSTEM_ERROR_DECODE_TYPE(e.getCodeError()) : 5200100);

                packet_func.session_send(p,
                    _session, 1);
            }

            return false;
        }

        public bool cancelEditShop(Player _session,ref PangyaBinaryWriter _out_packet)
        {

            Locker _locker = new Locker(this,
                _session.m_pi.uid,
                eTYPE_LOCK.TL_SELECT);

            PangyaBinaryWriter p = new PangyaBinaryWriter();

            try
            {

                PersonalShop ps = null;

                if ((ps = findShop(_session)) == null)
                {
                    throw new exception("[PersonalShopManager::cancelEditShop][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou cancela edit Personal Shop, mas ele nao tem nenhum na sala. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP_MANAGER,
                        400, 5200401));
                }

                ps.setState(PersonalShop.STATE.OPEN);

                _out_packet.init_plain((ushort)0xE3);

                _out_packet.WriteUInt32(1); // OK

                _out_packet.WritePStr(_session.m_pi.nickname);

                // sucesso, enviar o pacote para a sala toda
                return true;

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[PersonalShopManager::cancelEditShop][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain((ushort)0xE3);

                p.WriteUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.PERSONAL_SHOP_MANAGER) ? ExceptionError.STDA_SYSTEM_ERROR_DECODE_TYPE(e.getCodeError()) : 5200400);

                packet_func.session_send(p,
                    _session, 1);
            }

            return false;
        }

        public bool closeShop(Player _session,ref PangyaBinaryWriter _out_packet)
        {

            Locker _locker = new Locker(this,
                _session.m_pi.uid,
                eTYPE_LOCK.TL_DELETE);

            PangyaBinaryWriter p = new PangyaBinaryWriter();

            try
            {

                // Verifica se o player tem um shop aberto
                PersonalShop ps = null;

                if ((ps = findShop(_session)) == null)
                {
                    throw new exception("[PersonalShopManager::closeShop][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao tem um personal shop criado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP_MANAGER,
                        150, 5200151));
                }

                _smp.message_pool.push(new message("[PersonalShopManager::closeShop][Log] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] fechou o Personal Shop[Owner UID=" + Convert.ToString(ps.getOwner().m_pi.uid) + ", STATE=" + Convert.ToString(ps.getState()) + ", NAME=" + ps.getName() + ", Count Item=" + Convert.ToString(ps.getCountItem()) + ", Pang Sale=" + Convert.ToString(ps.getPangSale()) + "] na sala[numero=" + Convert.ToString(m_ri.numero) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Deleta shop
                delete_shop(_session);

                // Sucesso
                _out_packet.init_plain((ushort)0xE4);

                _out_packet.WriteUInt32(1);

                _out_packet.WritePStr(_session.m_pi.nickname);
                _out_packet.WriteUInt32(_session.m_pi.uid);

                // sucesso, enviar o pacote para a sala toda
                return true;

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[PersonalShopManager::closeShop][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain((ushort)0xE5);

                p.WriteUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.PERSONAL_SHOP_MANAGER) ? ExceptionError.STDA_SYSTEM_ERROR_DECODE_TYPE(e.getCodeError()) : 5200150);

                packet_func.session_send(p,
                    _session, 1);
            }

            return false;
        }

        public bool changeShopName(Player _session,
            string _name,
            ref PangyaBinaryWriter _out_packet)
        {

            Locker _locker = new Locker(this,
                _session.m_pi.uid,
                eTYPE_LOCK.TL_SELECT);

            PangyaBinaryWriter p = new PangyaBinaryWriter();

            try
            {

                if (_name.Length == 0)
                {
                    throw new exception("[PersonalShopManager::changeShopName][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o no do shop mas enviou uma string vazia. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP_MANAGER,
                        200, 5200201));
                }

                // Verifica se esse nome de shop já existe na sala, tirando o dele é claro
                if (hasNameInSomeShop(_name, _session.m_pi.uid))
                {
                    throw new exception("[PersonalShopManager::changeShopName][Log] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou trocar o name[value=" + _name + "] do Personal Shop dele, but already exists on room.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP_MANAGER,
                        201, 5200202));
                }

                PersonalShop ps = null;

                if ((ps = findShop(_session)) == null)
                {
                    throw new exception("[PersonalShopManager::changeShopName][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao tem um personal shop nessa sala. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP_MANAGER,
                        202, 5200203));
                }

                ps.setName(_name);

                _smp.message_pool.push(new message("[PersonalShopManager::changeShopName][Log] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] trocou o nome[VALUE=" + _name + "] do seu Personal Shop com sucesso!", type_msg.CL_FILE_LOG_AND_CONSOLE));

                _out_packet.init_plain((ushort)0xE8);

                _out_packet.WriteUInt32(1); // Ok

                _out_packet.WritePStr(ps.getName());

                _out_packet.WriteUInt32(_session.m_pi.uid);

                _out_packet.WritePStr(_session.m_pi.nickname);

                // sucesso, enviar o pacote para a sala toda
                return true;

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[PersonalShopManager::changeShopName][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain((ushort)0xE8);

                p.WriteUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.PERSONAL_SHOP_MANAGER) ? ExceptionError.STDA_SYSTEM_ERROR_DECODE_TYPE(e.getCodeError()) : 5200200);

                packet_func.session_send(p,
                    _session, 1);
            }

            return false;
        }

        public void openShop(Player _session, Packet _packet)
        {

            Locker _locker = new Locker(this,
                _session.m_pi.uid,
                eTYPE_LOCK.TL_SELECT);

            PangyaBinaryWriter p = new PangyaBinaryWriter();
            PersonalShopItem psi = new PersonalShopItem();

            try
            {

                uint count = _packet.ReadUInt32();

                if (count > 6)
                {
                    throw new exception("[PersonalShopManager::openShop][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou abrir Personal Shop com um numero[value=" + Convert.ToString(count) + "] de itens eh maior que o permitido", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP_MANAGER,
                        250, 5200251));
                }

                PersonalShop ps = null;

                if ((ps = findShop(_session)) == null)
                {
                    throw new exception("[PersonalShopManager::openShop][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou abrir um Personal Shop que ele nao tem.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP_MANAGER,
                        251, 5200252));
                }

                // Limpa os itens do Shop se tiver
                ps.clearItem();

                for (var i = 0u; i < count; ++i)
                {
                    psi.clear();

                    psi = _packet.Read<PersonalShopItem>();

                    // Dentro do push ele verifica se é permitido esse item no Personal Shop
                    ps.pushItem(psi);
                }

                // Abre o Shop
                ps.setState(PersonalShop.STATE.OPEN);

                _smp.message_pool.push(new message("[PersonalShopManager::openShop][Log] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] abriu o Personal Shop[NAME=" + ps.getName() + ", Count Item=" + Convert.ToString(ps.getCountItem()) + ", Pang Sale=" + Convert.ToString(ps.getPangSale()) + "] ", type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain((ushort)0xEB);

                p.WriteUInt32(1); // Ok

                p.WritePStr(_session.m_pi.nickname);

                p.WriteUInt32(_session.m_pi.uid);

                ps.putItemOnPacket(p);

                packet_func.session_send(p,
                    _session, 1);

            }
            catch (exception e)
            {

                if (ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
                    STDA_ERROR_TYPE.PERSONAL_SHOP,
                    23))
                {

                    p.init_plain((ushort)0x40); // Msg to Chat of player

                    p.WriteByte(7); // Notice

                    p.WritePStr(_session.m_pi.nickname);
                    p.WritePStr("Card price is outside the price range.");

                    packet_func.session_send(p,
                        _session, 1);
                }

                _smp.message_pool.push(new message("[PersonalShopManager::openShop][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain((ushort)0xEB);

                p.WriteUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.PERSONAL_SHOP_MANAGER) ? ExceptionError.STDA_SYSTEM_ERROR_DECODE_TYPE(e.getCodeError()) : 5200250);

                packet_func.session_send(p,
                    _session, 1);
            }
        }

        public void buyInShop(Player _session, Packet _packet)
        {

            Locker _locker = new Locker(this,
                _session.m_pi.uid,
                eTYPE_LOCK.TL_SELECT);

            PangyaBinaryWriter p = new PangyaBinaryWriter();

            try
            {

                uint owner_uid = _packet.ReadUInt32();

                PersonalShopItem psi =  _packet.Read<PersonalShopItem>();

                PersonalShop ps = null;

                if ((ps = findShop(owner_uid)) == null)
                {
                    throw new exception("[room::requestBuyItemSaleShop][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou comprar item[TYPEID=" + Convert.ToString(psi.item._typeid) + ", ID=" + Convert.ToString(psi.item.id) + "] no Shop[Owner UID=" + Convert.ToString(owner_uid) + "], mas ele nao tem um shop nesta nessa sala[numero=" + Convert.ToString(m_ri.numero) + "]. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP_MANAGER,
                        551, 5200552));
                }

                ps.buyItem(_session, psi);

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[PersonalShopManager::buyInShop][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain((ushort)0xEC);

                p.WriteUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.PERSONAL_SHOP_MANAGER) ? ExceptionError.STDA_SYSTEM_ERROR_DECODE_TYPE(e.getCodeError()) : 5200550);

                packet_func.session_send(p,
                    _session, 1);
            }
        }

        public void visitCountShop(Player _session)
        {

            Locker _locker = new Locker(this,
                _session.m_pi.uid,
                eTYPE_LOCK.TL_SELECT);

            PangyaBinaryWriter p = new PangyaBinaryWriter();

            try
            {

                PersonalShop ps = null;

                if ((ps = findShop(_session)) == null)
                {
                    throw new exception("[PersonalShopManager::visitCountShop][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou pedir visit count do Personal Shop, mas ele nao tem na sala. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP_MANAGER,
                        300, 5200301));
                }

                p.init_plain((ushort)0xE9);

                p.WriteUInt32(1); // OK

                p.WriteUInt32(ps.getVisitCount());

                packet_func.session_send(p,
                    _session, 1);

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[PersonalShopManager::visitCountShop][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain((ushort)0xE9);

                p.WriteUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.PERSONAL_SHOP_MANAGER) ? ExceptionError.STDA_SYSTEM_ERROR_DECODE_TYPE(e.getCodeError()) : 5200300);

                packet_func.session_send(p,
                    _session, 1);
            }
        }

        public void pangShop(Player _session)
        {

            Locker _locker = new Locker(this,
                _session.m_pi.uid,
                eTYPE_LOCK.TL_SELECT);

            PangyaBinaryWriter p = new PangyaBinaryWriter();

            try
            {

                PersonalShop ps = null;

                if ((ps = findShop(_session)) == null)
                {
                    throw new exception("[PersonalShopManager::pangShop][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou pedir pang sale do Personal Shop, mas ele nao tem na sala. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP_MANAGER,
                        350, 5200351));
                }

                p.init_plain((ushort)0xEA);

                p.WriteUInt32(1); // OK

                p.WriteUInt64(ps.getPangSale());

                packet_func.session_send(p,
                    _session, 1);

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[PersonalShopManager::pangShop][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain((ushort)0xEA);

                p.WriteUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.PERSONAL_SHOP_MANAGER) ? ExceptionError.STDA_SYSTEM_ERROR_DECODE_TYPE(e.getCodeError()) : 5200350);

                packet_func.session_send(p,
                    _session, 1);
            }
        }

        public void viewShop(Player _session, uint _owner_uid)
        {

            Locker _locker = new Locker(this,
                _session.m_pi.uid,
                eTYPE_LOCK.TL_SELECT);

            PangyaBinaryWriter p = new PangyaBinaryWriter();

            try
            {

                PersonalShop ps = null;

                if ((ps = findShop(_owner_uid)) == null)
                {
                    throw new exception("[PersonalShopManager::viewShop][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ver o Shop[Owner UID=" + Convert.ToString(_owner_uid) + "], mas ele nao tem um shop nesta nessa sala[numero=" + Convert.ToString(m_ri.numero) + "]. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP_MANAGER,
                        451, 5200452));
                }

                try
                {
                    // Add Client
                    ps.addClient(_session);
                }
                catch (exception e)
                {

                    if (ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
                        STDA_ERROR_TYPE.PERSONAL_SHOP,
                        6))
                    {

                        _smp.message_pool.push(new message("[PersonalShopManager::viewShop][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                        throw new exception("[PersonalShopManager::viewShop][Log] " + e.getFullMessageError(), ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP_MANAGER,
                            452, 5200453));
                    }
                    else
                    {
                        throw;
                    }
                }

                p.init_plain((ushort)0xE6);

                p.WriteUInt32(1); // OK

                p.WritePStr(ps.getOwner().m_pi.nickname);

                p.WritePStr(ps.getName());

                p.WriteUInt32(ps.getOwner().m_pi.uid);

                ps.putItemOnPacket(p);

                packet_func.session_send(p,
                    _session, 1);

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[PersonalShopManager::viewShop][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain((ushort)0xE6);

                p.WriteUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.PERSONAL_SHOP_MANAGER) ? ExceptionError.STDA_SYSTEM_ERROR_DECODE_TYPE(e.getCodeError()) : 5200450);

                packet_func.session_send(p,
                    _session, 1);
            }
        }

        public void closeViewShop(Player _session, uint _owner_uid)
        {

            Locker _locker = new Locker(this,
                _session.m_pi.uid,
                eTYPE_LOCK.TL_SELECT);

            PangyaBinaryWriter p = new PangyaBinaryWriter();

            try
            {

                PersonalShop ps = null;

                if ((ps = findShop(_owner_uid)) == null)
                {
                    throw new exception("[PersonalShopManager::closeViewShop][Error] player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou fechar o Shop[Owner UID=" + Convert.ToString(_owner_uid) + "], mas ele nao tem um shop nesta nessa sala[numero=" + Convert.ToString(m_ri.numero) + "]. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PERSONAL_SHOP_MANAGER,
                        501, 5200502));
                }

                // deleta viewer
                ps.deleteClient(_session);

                // Sucesso
                p.init_plain((ushort)0xE7);

                p.WriteUInt32(1); // OK

                packet_func.session_send(p,
                    _session, 1);

            }
            catch (exception e)
            {

                _smp.message_pool.push(new message("[PersonalShopManager::closeViewShop][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain((ushort)0xE7);

                p.WriteUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.PERSONAL_SHOP_MANAGER) ? ExceptionError.STDA_SYSTEM_ERROR_DECODE_TYPE(e.getCodeError()) : 5200500);

                packet_func.session_send(p,
                    _session, 1);
            }
        }

        protected void @lock()
        {
#if _WIN32
				EnterCriticalSection(m_cs);
#elif __linux__
				pthread_mutex_lock(m_cs);
#endif
        }

        protected void unlock()
        {
#if _WIN32
				LeaveCriticalSection(m_cs);
#elif __linux__
				pthread_mutex_unlock(m_cs);
#endif
        }

        protected void clear_shops()
        {

            try
            {

                // lock
                @lock();

                while (mapShop.Any())
                {

                    var it = mapShop.First();

                    if (it.Value.m_type != eTYPE_LOCK.TL_NONE && waitSpinDown())
                    {
                        continue;
                    }

                    if (it.Value.m_shop != null)
                    {
                        it.Value.m_shop = null;
                    }

                    it.Value.m_shop = null;

                    mapShop.Remove(it.Key);
                }

                // unlock
                unlock();

            }
            catch (exception e)
            {

                // unlock
                unlock();

                _smp.message_pool.push(new message("[PersonalShopManager::clear_shops][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        // unsafe thread
        protected bool _hasNameInSomeShop(string _name, uint _owner_uid)
        {
            return mapShop.Values.Any(_el =>
                _el.m_shop != null &&
                _el.m_shop.getOwner().m_pi.uid != _owner_uid &&
                _el.m_shop.getName() == _name);
        }


        // unsafe thread
        protected PersonalShop _findShop(Player _session)
        {

            var it = _findShopIt(_session);

            return (!mapShop.Any()) ? null : it.First().Value.m_shop;
        }

        protected PersonalShop _findShop(uint _owner_uid)
        {

            var it = _findShopIt(_owner_uid);

            return (!mapShop.Any()) ? null : it.First().Value.m_shop;
        }

        protected Dictionary<Player, PersonalShopCtx> _findShopIt(Player _session)
        {
            return mapShop
                .Where(shopCtx => shopCtx.Key == _session) // Filtra os elementos
                .ToDictionary(shopCtx => shopCtx.Key, shopCtx => shopCtx.Value); // Cria um novo dicionário
        }

        protected Dictionary<Player, PersonalShopCtx> _findShopIt(uint _owner_uid)
        {
            return mapShop
                .Where(shopCtx => shopCtx.Value.m_shop != null && shopCtx.Value.m_shop.getOwner().m_pi.uid == _owner_uid)
                .ToDictionary(shopCtx => shopCtx.Key, shopCtx => shopCtx.Value);
        }



        // unsafe thread
        protected void _delete_shop(Player _session)
        {
            if(mapShop.Any(c => c.Key == _session))
            _delete_shop(_session);
        }

        // C++ TO C# CONVERTER TASK: The implementation of the following method could not be found:
        //			void _delete_shop(System.Collections.Generic.SortedDictionaryIterator< player, PersonalShopCtx > _it_shop);

        protected bool waitSpinDown()
        {

#if _WIN32
				if(SleepConditionVariableCS(m_cv,
					m_cs, INFINITE) == 0)
				{

					uint error = GetLastError();

					LeaveCriticalSection((m_cs));

					if(error == ERROR_TIMEOUT)
					{
						_smp.message_pool.push(new message("[PersonalShopManager::waitSpinDown][Error] time out, but _milliseconds is INFINITE. wrong and unknown error. Error Code: " + Convert.ToString(error), type_msg.CL_FILE_LOG_AND_CONSOLE));
					} else
					{
						_smp.message_pool.push(new message("[PersonalShopManager::waitSpinDown][Error] ao receber o sinal da condition variable. Error Code: " + Convert.ToString(error), type_msg.CL_FILE_LOG_AND_CONSOLE));
					}

					return false;
				}
#elif __linux__
				int error = 0;
				if((error = pthread_cond_wait(m_cv, m_cs)) != 0)
				{

					pthread_mutex_unlock(m_cs);

					_smp.message_pool.push(new message("[PersonalShopManager::waitSpinDown][Error] ao receber sinal the condition variable. Error Code: " + Convert.ToString(error), type_msg.CL_FILE_LOG_AND_CONSOLE));

					return false;
				}
#endif

            return true;
        }

        protected void wakeAllLocked()
        {
#if _WIN32
				WakeAllConditionVariable(m_cv);
#elif __linux__
				pthread_cond_broadcast(m_cv);
#endif
        }

        protected void spinUp(uint _owner_uid, eTYPE_LOCK _type)
        {

            try
            {

                 bool bContinue = false;
                var it = new KeyValuePair<Player, PersonalShopCtx>();

                // lock
                @lock();

                do
                {

                    it = _findShopIt(_owner_uid).First();

                    if (it.Value == null)
                    {
                        break;
                    } 

                    if (it.Value.m_type == eTYPE_LOCK.TL_NONE)
                    {

                        it.Value.m_type = _type;
                        it.Value.m_count = 1;

                    }
                    else if (it.Value.m_type == eTYPE_LOCK.TL_SELECT && _type == eTYPE_LOCK.TL_SELECT)
                    {
                        it.Value.m_count++;
                    }
                    else
                    {
                        bContinue = waitSpinDown();
                    }

                } while (bContinue);

                // unlock
                unlock();

            }
            catch (exception e)
            {

                // unlock
                unlock();

                _smp.message_pool.push(new message("[PersonalShopManager::spinUp][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        protected void spinDown(uint _owner_uid, eTYPE_LOCK _type)
        {

            try
            {

                // C++ TO C# CONVERTER TASK: The typedef 'mapShop' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
                var it = new KeyValuePair<Player, PersonalShopCtx>();

                // lock
                @lock();

                do
                {

                    it = _findShopIt(_owner_uid).First();

                    if (it.Value == null)
                    {
                        break;
                    }

                    if (it.Value.m_type != _type)
                    {
                        _smp.message_pool.push(new message("[PersonalShopManager::spinDown][Error] type lock not match, CTX_TYPE: " + Convert.ToString((uint)it.Value.m_type) + " != " + Convert.ToString((uint)_type), type_msg.CL_FILE_LOG_AND_CONSOLE));
                    }

                    if (it.Value.m_count <= 1)
                    {

                        it.Value.m_count = 0;
                        it.Value.m_type = eTYPE_LOCK.TL_NONE;

                        wakeAllLocked();

                    }
                    else
                    {
                        it.Value.m_count--;
                    }

                } while (false);

                // unlock
                unlock();

            }
            catch (exception e)
            {

                // unlock
                unlock();

                _smp.message_pool.push(new message("[PersonalShopManager::spinDown][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        protected Dictionary<Player, PersonalShopCtx> mapShop = new Dictionary<Player, PersonalShopCtx>();

        // Owner room info
        protected RoomInfoEx m_ri;
    }
}