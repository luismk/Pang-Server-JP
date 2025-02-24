using GameServer.GameType;
using PangyaAPI.Utilities;
using System.Collections.Generic;
using static GameServer.GameType._Define;
using System;
using _smp = PangyaAPI.Utilities.Log;
using GameServer.Session;
using PangyaAPI.Utilities.Log;
namespace GameServer.Game.Manager
{
    public class RoomManager
    {
        public RoomManager(byte _channel_id)
        {
            this.m_channel_id = _channel_id;
        }

        public void destroy()
        {


            foreach (var el in v_rooms)
            {

                if (el != null)
                {

                    // Sala está destruindo
                 //   el.setDestroying();

                    // Libera a sala se ela estiver bloqueada
                 //   el.unlock();

                     
                }
            }

            v_rooms.Clear();
            m_channel_id = 255;
        }

        public room makeRoom(byte _channel_owner,
            RoomInfoEx _ri,
            Player _session,
            int _option = 0)
        {
            room r = null;

            try
            {



                if (_session != null && _session.m_pi.mi.sala_numero != ushort.MaxValue)
                {
                    throw new exception("[RoomManager::makeRoom][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] sala[NUMERO=" + Convert.ToString(_session.m_pi.mi.sala_numero) + "], ja esta em outra sala, nao pode criar outra. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM_MANAGER,
                        120, 0));
                }

                _ri.numero = getNewIndex();

                if (_option == 0 && _session != null)
                {
                    _ri.master = (int)_session.m_pi.uid;
                }
                else if (_option == 1) // Room Sem Master Grand Prix ou Grand Zodiac Event Time
                {
                    _ri.master = -2;
                }
                else // Room sem master
                {
                    _ri.master = -1;
                }

                r = new room(_channel_owner, _ri);

                if (r == null)
                {
                    throw new exception("[RoomManager::makeRoom][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou criar a sala[TIPO=" + Convert.ToString((ushort)_ri.tipo) + "], mas nao conseguiu criar o objeto da classe room. Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM_MANAGER,
                        130, 0));
                }

                // Verifica se é um room válida e bloquea ela
                // C++ TO C# CONVERTER TASK: The #define macro '//WAIT_ROOM_UNLOCK' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
                //WAIT_ROOM_UNLOCK("makeRoom");

                // Adiciona a sala no Vector
                v_rooms.Add(r);

                if (_session != null)
                {
                    r.enter(_session);
                }

                // Log
                _smp.message_pool.push(new message("[RoomManager::makeRoom][Log] Channel[ID=" + Convert.ToString((ushort)m_channel_id) + "] Maked Room[TIPO=" + Convert.ToString((ushort)r.getInfo().tipo) + ", NUMERO=" + Convert.ToString(r.getNumero()) + ", MASTER=" + Convert.ToString((int)r.getMaster()) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                //_smp::message_pool::getInstance().push(new message("Key Room Dump.\n\r" + hex_util::BufferToHexString((unsigned char*)r->getInfo()->key, 16), type_msg.CL_FILE_LOG_AND_CONSOLE));



            }
            catch (exception e)
            {

                // Libera Crictical Session do Room Manager


                if (r != null)
                {

                    // Destruindo a sala, não conseguiu
                    r.setDestroying();

                    // Desbloqueia para
                    if (!ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
                        STDA_ERROR_TYPE.ROOM, 150))
                    {
                        r.unlock();
                    }

                    // Deletando o Objeto
                    r = null;

                    // Limpa o ponteiro
                    r = null;
                }

                _smp.message_pool.push(new message("[RoomManager::makeRoom][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return r;
        }

        public void destroyRoom(room _room)
        {

            if (_room == null)
            {
                throw new exception("[RoomManager::destroyRoom][Error] _room is nullptr.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM_MANAGER,
                    4, 0));
            }

            int index = findIndexRoom(_room);

            if (index == (int)~0)
            {
                throw new exception("[RoomManager::destroyRoom][Error] room nao existe no vector de salas.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM_MANAGER,
                    5, 0));
            }

            string log = "";

            try
            {



                clearIndex(_room.getNumero());

                v_rooms.RemoveAt(index);

                // Make Log
                log = "[RoomManager::destroyRoom][Log] Channel[ID=" + Convert.ToString((ushort)m_channel_id) + "] Room[TIPO=" + Convert.ToString((ushort)_room.getInfo().tipo) + ", NUMERO=" + Convert.ToString(_room.getNumero()) + ", MASTER=" + Convert.ToString((int)_room.getMaster()) + "] destroyed.";

                // Sala vai ser deletada
                _room.setDestroying();

                // Vai destruir(excluir) a sala, libera a sala
                _room.unlock();

                _room = null; // Libera memória alocada para a sala

                // Deletou a sala;
                _room = null;

                // Show Log, se destruiu a sala com sucesso
                _smp.message_pool.push(new message(log, type_msg.CL_FILE_LOG_AND_CONSOLE));



            }
            catch (exception e)
            {

                // Libera Crictical Session do Room Manager


                if (_room != null)
                {

                    _room.setDestroying();

                    _room.unlock();

                    _room = null;

                    _room = null;
                }

                _smp.message_pool.push(new message("[RoomManager::destroy][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        // Make room Grand Prix
        public RoomGrandPrix makeRoomGrandPrix(byte _channel_owner,
            RoomInfoEx _ri,
            Player _session,
            PangLib.IFF.JP.Models.Data.GrandPrixData _gp,
            int _option = 0)
        {

            RoomGrandPrix r = null;

            try
            {



                if (_session != null && _session.m_pi.mi.sala_numero != ushort.MaxValue)
                {
                    throw new exception("[RoomManager::makeRoomGrandPrix][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] sala[NUMERO=" + Convert.ToString(_session.m_pi.mi.sala_numero) + "], ja esta em outra sala, nao pode criar outra. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM_MANAGER,
                        120, 0));
                }

                _ri.numero = getNewIndex();

                if (_option == 0 && _session != null)
                {
                    _ri.master = (int)_session.m_pi.uid;
                }
                else if (_option == 1) // Room Sem Master Grand Prix ou Grand Zodiac Event Time
                {
                    _ri.master = -2;
                }
                else // Room sem master
                {
                    _ri.master = -1;
                }

                r = new RoomGrandPrix(_channel_owner,
                    _ri, _gp);

                if (r == null)
                {
                    throw new exception("[RoomManager::makeRoomGrandPrix][Error] Player[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou criar a sala[TIPO=" + Convert.ToString((ushort)_ri.tipo) + "], mas nao conseguiu criar o objeto da classe RoomGrandPrix. Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM_MANAGER,
                        130, 0));
                }

                // Verifica se é um room grand prix válida e bloquea ela
                // C++ TO C# CONVERTER TASK: The #define macro '//WAIT_ROOM_GP_UNLOCK' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
                //WAIT_ROOM_GP_UNLOCK("makeRoomGrandPrix");

                // Adiciona a sala ao vector
                v_rooms.Add(r);

                if (_session != null)
                {
                    r.enter(_session);
                }

                // Log
                _smp.message_pool.push(new message("[RoomManager::makeRoomGrandPrix][Log] Channel[ID=" + Convert.ToString((ushort)m_channel_id) + "] Maked Room[TIPO=" + Convert.ToString((ushort)r.getInfo().tipo) + ", NUMERO=" + Convert.ToString(r.getNumero()) + ", MASTER=" + Convert.ToString((int)r.getMaster()) + ", PLAYER_REQUEST_CREATE=" + (_session != null ? Convert.ToString(_session.m_pi.uid) : "NENHUMA-SYSTEM") + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                //_smp::message_pool::getInstance().push(new message("Key Room Dump.\n\r" + hex_util::BufferToHexString((unsigned char*)r->getInfo()->key, 16), type_msg.CL_FILE_LOG_AND_CONSOLE));



            }
            catch (exception e)
            {

                // Libera Crictical Session do Room Manager


                if (r != null)
                {

                    // Destruindo a sala, não conseguiu
                    r.setDestroying();

                    // Desbloqueia para
                    if (!ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
                        STDA_ERROR_TYPE.ROOM, 150))
                    {
                        r.unlock();
                    }

                    // Deletando o Objeto
                    r = null;

                    // Limpa o ponteiro
                    r = null;
                }

                _smp.message_pool.push(new message("[RoomManager::makeRoomGrandPrix][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return r;
        }

        // Make room Grand Zodiac Event
        public RoomGrandZodiacEvent makeRoomGrandZodiacEvent(byte _channel_owner, RoomInfoEx _ri)
        {

            RoomGrandZodiacEvent r = null;

            try
            {



                _ri.numero = getNewIndex();

                // Room Sem Master Grand Prix ou Grand Zodiac Event Time
                _ri.master = -2;

                r = new RoomGrandZodiacEvent(_channel_owner, _ri);

                if (r == null)
                {
                    throw new exception("[RoomManager::makeRoomGrandZodiacEvent][Error] tentou criar a sala[TIPO=" + Convert.ToString((ushort)_ri.tipo) + "] Grand Zodiac Event, mas nao conseguiu criar o objeto da classe room. Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM_MANAGER,
                        130, 0));
                }

                // Verifica se é um room válida e bloquea ela
                // C++ TO C# CONVERTER TASK: The #define macro '////WAIT_ROOM_GZE_UNLOCK' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
                ////WAIT_ROOM_GZE_UNLOCK("makeRoomGrandZodiacEvent");

                // Adiciona a sala no Vector
                v_rooms.Add(r);

                // Log
                _smp.message_pool.push(new message("[RoomManager::makeRoomGrandZodiacEvent][Log] Channel[ID=" + Convert.ToString((ushort)m_channel_id) + "] Maked Room[TIPO=" + Convert.ToString((ushort)r.getInfo().tipo) + ", NUMERO=" + Convert.ToString(r.getNumero()) + ", MASTER=" + Convert.ToString((int)r.getMaster()) + "] Grand Zodiac Event.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                //_smp::message_pool::getInstance().push(new message("Key Room Dump.\n\r" + hex_util::BufferToHexString((unsigned char*)r->getInfo()->key, 16), type_msg.CL_FILE_LOG_AND_CONSOLE));



            }
            catch (exception e)
            {

                // Libera Crictical Session do Room Manager


                if (r != null)
                {

                    // Destruindo a sala, não conseguiu
                    r.setDestroying();

                    // Desbloqueia para
                    if (!ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
                        STDA_ERROR_TYPE.ROOM, 150))
                    {
                        r.unlock();
                    }

                    // Deletando o Objeto
                    r = null;

                    // Limpa o ponteiro
                    r = null;
                }

                _smp.message_pool.push(new message("[RoomManager::makeRoomGrandZodiacEvent][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return r;
        }

        // Make room Bot GM Event
        public RoomBotGMEvent makeRoomBotGMEvent(byte _channel_owner,
            RoomInfoEx _ri,
            List<stReward> _rewards)
        {

            RoomBotGMEvent r = null;

            try
            {



                _ri.numero = getNewIndex();

                // Room Sem Master Grand Prix ou Grand Zodiac Event Time ou Bot GM Event
                _ri.master = -2;

                r = new RoomBotGMEvent(_channel_owner,
                    _ri, _rewards);

                if (r == null)
                {
                    throw new exception("[RoomManager::makeRoomBotGMEvent][Error] tentou criar a sala[TIPO=" + Convert.ToString((ushort)_ri.tipo) + "] Bot GM Event, mas nao conseguiu criar o objeto da classe room. Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM_MANAGER,
                        130, 0));
                }

                // Adiciona a sala no Vector
                v_rooms.Add(r);

                // Log
                _smp.message_pool.push(new message("[RoomManager::makeRoomBotGMEvent][Log] Channel[ID=" + Convert.ToString((ushort)m_channel_id) + "] Maked Room[TIPO=" + Convert.ToString((ushort)r.getInfo().tipo) + ", NUMERO=" + Convert.ToString(r.getNumero()) + ", MASTER=" + Convert.ToString((int)r.getMaster()) + "] Bot GM Event.", type_msg.CL_FILE_LOG_AND_CONSOLE));

            }
            catch (exception e)
            {

                // Libera Crictical Session do Room Manager


                if (r != null)
                {

                    // Destruindo a sala, não conseguiu
                    r.setDestroying();

                    // Desbloqueia para
                    if (!ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
                        STDA_ERROR_TYPE.ROOM, 150))
                    {
                        r.unlock();
                    }

                    // Deletando o Objeto
                    r = null;

                    // Limpa o ponteiro
                    r = null;
                }

                _smp.message_pool.push(new message("[RoomManager::makeRoomBotGMEvent][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return r;
        }

        public room findRoom(short _numero)
        {

            if (_numero == -1)
            {
                return null;
            }

            room r = null;

            try
            {



                for (var i = 0; i < v_rooms.Count; ++i)
                {
                    if (v_rooms[i].getNumero() == _numero)
                    {
                        r = v_rooms[i];
                        break;
                    }
                }
            }
            catch (exception e)
            {

                // Libera Crictical Session do Room Manager


                if (r != null)
                {

                    if (!ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
                        STDA_ERROR_TYPE.ROOM, 150))
                    {
                        r.unlock();
                    }

                    r = null;
                }

                _smp.message_pool.push(new message("[RoomManager::findRoom][ErrorSystem] " + e.getFullMessageError()));
            }

            return r;
        }

        public RoomGrandPrix findRoomGrandPrix(uint _typeid)
        {

            if (_typeid == 0)
            {
                return null;
            }

            RoomGrandPrix r = null;

            try
            {



                foreach (var el in v_rooms)
                {

                    if (el.getInfo().grand_prix.active
                        && el.getInfo().grand_prix.dados_typeid != 0U
                        && el.getInfo().grand_prix.dados_typeid == _typeid)
                    {

                        r = (RoomGrandPrix)el;

                        break;
                    }
                }

            }
            catch (exception e)
            {

                // Libera Crictical Session do Room Manager


                if (r != null)
                {

                    if (!ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
                        STDA_ERROR_TYPE.ROOM, 150))
                    {
                        r.unlock();
                    }

                    r = null;
                }

                _smp.message_pool.push(new message("[RoomManager::findRoomGrandPrix][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return r;
        }

        // Opt sem sala practice, se não todas as salas
        public List<RoomInfo> getRoomsInfo(bool _without_practice_room = true)
        {

            List<RoomInfo> v_ri = new List<RoomInfo>();

            for (var i = 0; i < v_rooms.Count; ++i)
            {
                if (v_rooms[i] != null && (!_without_practice_room || (v_rooms[i].getInfo().tipo != RoomInfo.TIPO.PRACTICE && v_rooms[i].getInfo().tipo != RoomInfo.TIPO.GRAND_ZODIAC_PRACTICE)))
                {
                    v_ri.Add((RoomInfo)v_rooms[i].getInfo());
                }
            }
            return v_ri;
        }

        public List<RoomGrandZodiacEvent> getAllRoomsGrandZodiacEvent()
        {

            List<RoomGrandZodiacEvent> v_r = new List<RoomGrandZodiacEvent>(); 

            foreach (var el in v_rooms)
            {
                if (el != null
                    && (int)el.getMaster() == -2
                    && (el.getInfo().tipo == RoomInfo.TIPO.GRAND_ZODIAC_ADV || el.getInfo().tipo == RoomInfo.TIPO.GRAND_ZODIAC_INT))
                {
                    //v_r.Add(reinterpret_cast<RoomGrandZodiacEvent>(el));
                }
            }
            return v_r;
        }

        public List<RoomBotGMEvent> getAllRoomsBotGMEvent()
        {

            List<RoomBotGMEvent> v_r = new List<RoomBotGMEvent>();

            foreach (var el in v_rooms)
            {
                if (el != null
                    && (int)el.getMaster() == -2
                    && (RoomInfo.TIPO)el.getInfo().tipo == RoomInfo.TIPO.TOURNEY
                    && el.getInfo().flag_gm == 1
                    && el.getInfo().trofel == TROFEL_GM_EVENT_TYPEID)
                {
                    // C++ TO C# CONVERTER TASK: There is no equivalent to 'reinterpret_cast' in C#:
                    //	v_r.Add(reinterpret_cast<RoomBotGMEvent>(el));
                }
            }
            return v_r;
        }

        // Unlock Room
        public void unlockRoom(room _r)
        {

            // _r is invalid
            if (_r == null)
            {
                return;
            }

            try
            {



                foreach (var el in v_rooms)
                {

                    if (el != null && el == _r)
                    {

                        // Libera a sala
                        el.unlock();

                            // Acorda as outras threads que estão esperando                 
							break;
						}
					}



				} catch(exception e)
				{



					_smp.message_pool.push(new message("[RoomManager::unlockRoom][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
				}
			}

			protected int findIndexRoom(room _room)
			{

				if(_room == null)
				{
					throw new exception("[RoomManager::findIndexRoom][Error] _room is nullptr.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM_MANAGER,
						4, 0));
				}

				int index = ~ 0;
 
				for(var i = 0; i < v_rooms.Count; ++ i)
				{
					if(v_rooms[i] == _room)
					{
						index = i;
						break;
					}
				} 
				return index;
			}

			private ushort getNewIndex()
			{

				ushort index = 0; 
				for(ushort i = 0; i < ushort.MaxValue; ++ i)
				{
					if(m_map_index[i] == 0)
					{
						index = i;
						m_map_index[i] = 1; // Ocupado
						break;
					}
				} 

				return index;
			}

			private void clearIndex(short _index)
			{

				if(_index >= short.MaxValue)
				{
					throw new exception("[RoomManager::clearIndex][Error] _index maior que o limite do mapa de indexes.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM_MANAGER,
						3, 0));
				}
 
				m_map_index[_index] = 0; // Livre 
			}

			// Member
			private ushort[] m_map_index = new ushort[ushort.MaxValue];

			// Dodo do Objeto RoomManager Channel ID
			private byte m_channel_id;

			protected List< room > v_rooms = new List< Room >(); 
	}
}
