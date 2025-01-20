using GameServer.GameServerTcp;
using System;
using System.Collections.Generic;
using System.Linq;
using _smp = PangyaAPI.Utilities.Log;
using packet = PangyaAPI.TCP.PangyaPacket.Packet;
using PangyaAPI.TCP.Pangya_St;
using GameServer.PangType;
using System.Runtime.InteropServices;
using GameServer.Game;
using PangyaAPI.Utilities;
using static GameServer.PangType._Define;
using MemberInfo = GameServer.PangType.MemberInfo;
using SYSTEMTIME = GameServer.PangType.PangyaTime;
using GameServer.Session;
using PangyaAPI.TCP.Session;
using PangyaAPI.TCP.PangyaPacket;
using GameServer.Game.RoomClass;
using PangyaAPI.TCP.PangyaServer;
using PangyaAPI.Utilities.Log;
using System.Reflection;
using System.Runtime.Remoting.Channels;

namespace GameServer.PacketFunc
{
    public static class packet_func
    {
        static int MAX_BUFFER_PACKET = 1000;
        static int total;
        static int por_packet;
        static packet m_p;
        static multimap<uint, WarehouseItemEx> m_element;
        static int elements;
        static int m_element_size;
        static int max_packet = MAX_BUFFER_PACKET;
        #region Call Packet
        static GameServerTcp.GameServer gs = Program.gs;
        public static void packet002(ParamDispatch _arg1)
        {
            try
            {
                gs.requestLogin((Player)_arg1._session, _arg1._packet);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[packet_func_gs::packet002][StError]: {ex.Message}");
            }
        }

        public static void packet003(ParamDispatch pd)
        {

            try
            {

                gs.requestChat((Player)pd._session, pd._packet);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[packet_func_gs::packet003][StError]: {ex.Message}");
            }
        }

        public static void packet004(ParamDispatch pd)
        {
            try
            {
                // Enter Channel, channel ID
                gs.requestEnterChannel((Player)pd._session, pd._packet);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[packet_func_gs::packet004][StError]: {ex.Message}");
            }
        }


        public static void packet081(ParamDispatch pd)
        {
            var player = ((Player)pd._session);
            try
            {
                var c = gs.findChannel(player.m_pi.channel);

                if (c != null)
                    c.requestEnterLobby(player, pd._packet);
            }
            catch (exception)
            {
            }
        }
        public static void packet082(ParamDispatch pd)
        {
            var player = ((Player)pd._session);

            try
            {
                var c = gs.findChannel(player.m_pi.channel);

                if (c != null)
                    c.requestExitLobby(player, pd._packet);
            }
            catch (exception)
            {
            }
        }
        #endregion

        #region Response Packet
        public static void principal(ref packet p, PlayerInfo pi, ServerInfoEx _si)
        {
            try
            {
                if (pi == null)
                    throw new Exception("Erro PlayerInfo *pi is null. packet_func::principal()");

                int st_i = 0;
                PangyaTime si = new PangyaTime();
                si.CreateTime();
                p.WritePStr(_si.version_client);

                // member info
                p.WriteInt16(pi.mi.sala_numero);

                // Adiciona os dados do membro
                p.AddBuffer(pi.mi, new MemberInfo());

                // User Info
                p.WriteUInt32(pi.uid);
                p.AddBuffer(pi.ui, new UserInfo());

                // Trofel Info
                p.AddBuffer(pi.ti_current_season, new TrofelInfo());

                // User Equip
                p.AddBuffer(pi.ue, new UserEquip());

                // Map Statistics Normal
                for (st_i = 0; st_i < MS_NUM_MAPS; st_i++)
                {
                    p.AddBuffer(pi.a_ms_normal[st_i], new MapStatistics());
                }

                // Map Statistics Natural
                for (st_i = 0; st_i < MS_NUM_MAPS; st_i++)
                {
                    p.AddBuffer(pi.a_ms_natural[st_i], new MapStatistics());
                }

                // Map Statistics Grand Prix
                for (st_i = 0; st_i < MS_NUM_MAPS; st_i++)
                {
                    p.AddBuffer(pi.a_ms_grand_prix[st_i], new MapStatistics());
                }

                // Map Statistics Normal for all seasons
                for (int j = 0; j < 9; j++)
                {
                    for (st_i = 0; st_i < MS_NUM_MAPS; st_i++)        //talvez algum problema aqui!
                    {
                        p.AddBuffer(pi.aa_ms_normal_todas_season[j][st_i], new MapStatistics());
                    }
                }

                // Character Info (CharEquip)
                if (pi.ei.char_info != null)
                {
                    p.AddBuffer(pi.ei.char_info, new CharacterInfo());
                }
                else
                {
                    p.WriteZero(Marshal.SizeOf(new CharacterInfo()));
                }

                // CWriteie Info
                if (pi.ei.cad_info != null)
                {
                    p.AddBuffer(pi.ei.cad_info, new CharacterInfo());
                }
                else
                {
                    p.WriteZero(Marshal.SizeOf(new CharacterInfo()));
                }

                // Club Set Info
                p.AddBuffer(pi.ei.csi, new ClubSetInfo());

                // Mascot Info
                if (pi.ei.mascot_info != null)
                {
                    p.AddBuffer(pi.ei.mascot_info, new MascotInfo());
                }
                else
                {
                    p.WriteZero(Marshal.SizeOf(new MascotInfo()));
                }

                // Adiciona o buffer de tempo do Pangya
                p.AddBuffer(si, new PangyaTime());

                // Config do Server
                p.WriteInt16(1); // Valor padrão, 1 na primeira vez, 2 para logins subsequentes
                p.AddBuffer(pi.mi.papel_shop, pi.mi.papel_shop);
                p.WriteInt32(0); // Valor novo no JP, indicado como 0 em novas contas
                p.WriteUInt64(pi.block_flag.m_flag.ullFlag); // Flag do server para bloquear sistemas
                p.WriteUInt32(0); // Quantidade de vezes que logou
                p.WriteUInt32(_si.propriedade.ulProperty); // Propriedade do servidor
            }
            catch (Exception e)
            {
                _smp.message_pool.push("[packet_func_gs::principal]", e);
            }
        }

        public static void pacote046(ref packet p, Player _session, List<PlayerCanalInfo> v_element, int option)
        {

            var elements = v_element.Count();

            if (elements * Marshal.SizeOf(new PlayerCanalInfo()) < (MAX_BUFFER_PACKET - 100))
            {
                p.Write(new byte[] { 0x46 , 0x00});
                p.WriteByte((byte)option);
                p.WriteByte((byte)elements);

                for (int i = 0; i < v_element.Count(); i++)
                    p.AddBuffer(v_element[i], new PlayerCanalInfo());
            }
            else
            {
                //MAKE_BEGIN_SPLIT_PACKET(0x46, _session, MAX_BUFFER_PACKET);

                //p.WriteByte((byte)option);
                //p.WriteByte((byte)((total > por_packet) ? por_packet : total));

                //MAKE_MID_SPLIT_PACKET_VECTOR();

                //MAKE_END_SPLIT_PACKET(1);
            }

        }
        internal static void pacote11F(ref packet p, Player Player, PlayerInfo pi, short tipo)
        {
            if (pi == null)
                throw new Exception("Erro PlayerInfo *pi is nullptr. packet_func::pacote11F()");

            p.init_plain(0x11F);

            p.WriteInt16(tipo);

            p.AddBuffer(pi.TutoInfo, new TutorialInfo());
        }

        internal static void pacote1A9(ref packet p, Player Player, int ttl_milliseconds/*time to live*/, int option = 0)
        {
            p.init_plain(0x1A9);

            p.WriteByte((byte)option);

            p.WriteInt32(ttl_milliseconds);
        }

        internal static void pacote095(ref packet p, Player session, short sub_tipo, int option = 0, PlayerInfo pi = null)
        {
            p.init_plain(0x95);

            p.WriteInt16(sub_tipo);

            if (sub_tipo == 0x102)
                p.WriteByte((byte)option);

            else if (sub_tipo == 0x111)
            {
                p.WriteInt32(option);

                if (pi == null)
                {
                    //delete p;

                    throw new Exception("Erro PlayerInfo *pi is nullptr. packet_func::pacote095()");
                }

                p.WriteUInt64(pi.ui.pang);
            }
        }

        public static void pacote25D(ref packet p, Player m_session, List<TrofelEspecialInfo> v_tgp_current_season, int v)
        {
            // throw new NotImplementedException();
        }

        public static void pacote158(ref packet p, Player m_session, uint _uid, UserInfoEx _ui, byte season)
        {
            p.init_plain(0x158);

            p.WriteByte((byte)season);

            p.WriteUInt32(_uid);

            p.AddBuffer(_ui, new UserInfo());
        }

        public static void pacote096(ref packet p, Player m_session, PlayerInfo pi)
        {
            if (pi == null)
                throw new Exception("Erro PlayerInfo *pi is nullptr. packet_func::pacote096()");
                                      
            p.Write(new byte[] { 0x96, 0x00 });

            p.WriteUInt64(pi.cookie);

            // throw new NotImplementedException();
        }

        public static void pacote181(ref packet p, Player m_session, List<ItemBuffEx> v_ib)
        {
            // throw new NotImplementedException();
        }

        public static void pacote13F(ref packet p, Player m_session)
        {
            // throw new NotImplementedException();
        }

        public static void pacote137(ref packet p, Player m_session, List<CardEquipInfoEx> v_cei)
        {
            // throw new NotImplementedException();
        }

        public static void pacote136(ref packet p, Player m_session)
        {
            // throw new NotImplementedException();
        }

        public static void pacote138(ref packet p, Player m_session, List<CardInfo> v_card_info)
        {
            // throw new NotImplementedException();
        }

        public static void pacote135(ref packet p, Player m_session)
        {
            // throw new NotImplementedException();
        }

        public static void pacote131(ref packet p)
        {
            // throw new NotImplementedException();
        }

        public static void pacote072(ref packet p, Player m_session, UserEquip ue)
        {
            p.init_plain(0x72);

            p.AddBuffer(ue, new UserEquip());
        }

        public static void pacote0E1(ref packet p, Player m_session, SortedList<uint, MascotInfoEx> v_element, int option = 0)
        {
            p.init_plain(0xE1);

            p.WriteByte((byte)(v_element.Count & 0xFF));

            foreach (var item in v_element.Values)
                p.AddBuffer(item, new MascotInfo());
        }

        public static void pacote073(ref packet p, Player m_session, multimap<uint, WarehouseItemEx> v_element, int option = 0)
        {
            p.init_plain(0x73);
            p.WriteInt16((short)elements);
            p.WriteInt16((short)elements);

            foreach (var item in v_element.GetValues())
            {
                p.AddBuffer(item, new WarehouseItem());
            }

        }

        public static void pacote071(ref packet p, Player m_session, SortedList<uint, CaddieInfoEx> v_element, int option = 0)
        {
            var elements = v_element.Count();
            p.init_plain(0x71);
            p.WriteInt16((short)elements);
            p.WriteInt16((short)elements);

            foreach (var item in v_element.Values)
            {
                p.WriteStruct(item);
            }
        }

        public static void pacote070(ref packet p, Player m_session, SortedList<uint, CharacterInfoEx> v_element, int option = 0)
        {
            p.init_plain(0x70);
            p.WriteInt16((short)elements);
            p.WriteInt16((short)elements);

            foreach (var item in v_element.Values)
            {
                p.WriteStruct(item);
            }
        }


        public static void pacote04D(ref packet p, Player m_session, List<Channel> v_element, int option = 0)
        {

            p.Write(new byte[] { 0x4D , 0x00, (byte)v_element.Count });    
            for (var i = 0; i < v_element.Count; ++i)
                p.AddBuffer(v_element[i].getInfo(), new ChannelInfo());     
        }

        public static void pacote04E(ref packet p, Player session, int option, int _codeErrorInfo = 0)
        {
            /* Option Values
                * 1 Sucesso
                * 2 Channel Full
                * 3 Nao encontrou canal
                * 4 Nao conseguiu pegar informções do canal
                * 6 ErrorCode Info
                */

            p.init_plain(0x4E);

            p.WriteByte((byte)option);

            if (_codeErrorInfo != 0)
                p.WriteInt32(_codeErrorInfo);
        }

        public static void pacote044(ref packet p, Player _session, ServerInfoEx _si, int option, PlayerInfo pi = null, int valor = 0)
        {
            if (option == 0 && pi == null)
                throw new Exception("Erro PlayerInfo *pi is nullptr. packet_func::pacote044()");

            p.WriteByte(0x44);

            p.WriteByte((byte)(option & 0xFF));   // Option

            if (option == 0)
                principal(ref p, pi, _si);
            else if (option == 1)
                p.WriteByte(0);
            else if (option == 0xD3)
                p.WriteByte(0);
            else if (option == 0xD2)
                p.WriteInt32(valor);
        }

        public static int pacote0B2(ref packet p,
Player _session,
List<MsgOffInfo> v_element,
int option = 0)
        {

            p.init_plain((ushort)0xB2);

            p.WriteInt32(2); // Não sei bem o que é, mas pode ser uma opção

            p.WriteInt32(option);

            p.WriteUInt32((uint)v_element.Count);

            foreach (MsgOffInfo i in v_element)
            {
                p.AddBuffer(i, new MsgOffInfo());
            }

            return 0;
        }
        public static int pacote0D4(ref packet p,
Player _session,
SortedList<uint, CaddieInfoEx> v_element)
        {

            p.init_plain((ushort)0xD4);

            p.WriteUInt32((uint)v_element.Count());

            for (var i = 0; i != v_element.Count; i++)
            {
                p.AddBuffer(i, new CharacterInfo());
            }

            return 0;
        }
        public static void session_send(ref packet p, Player s, int _debug)
        {
            if (s == null)
                throw new Exception("[packet_func::session_send][Error] session *s is nullptr.");
            if(_debug == 1)
            _smp.message_pool.push($"[SEND_PACKET_LOG]: PacketSize({p.GetBytes().Length}) \t\n{p.GetBytes().HexDump()}");

            s.SendBuffer(p.GetBytes());

            p.Clear();//@! pode esta causando falha aqui
        }


        // Metôdos de auxílio de criação de pacotes
        public static void channel_broadcast(Channel _channel, ref packet p, int _debug)
        {

            vector<Player> channel_session = _channel.getSessions();  //gs->getSessionPool().getChannelSessions(s->m_channel);

            for (var i = 0; i < channel_session.Count(); ++i)
            {
                channel_session[i].SendBuffer(p.GetBytes());//@!errado
            }

            p.Clear();
        }

        public static void lobby_broadcast(Channel _channel, packet p, byte _debug)
        {

            vector<Player> channel_session = _channel.getSessions();  //gs->getSessionPool().getChannelSessions(s->m_channel);

            for (var i = 0; i < channel_session.Count(); ++i)
            {
                if (channel_session[i].m_pi.mi.sala_numero == -1)
                {
                }
            }

            //delete p;
        }

        public static int pacote210(ref packet p,
                Player _session,
                List<MailBox> v_element,
                int option = 0)
        {

            p.init_plain((ushort)0x210);

            p.WriteInt32(option);

            p.WriteInt32(v_element.Count);

            for (var i = 0; i < v_element.Count; ++i)
            {
                p.AddBuffer(v_element[i], new MailBox());
            }

            return 0;
        }

        internal static int pacote101(ref packet p, Player _session, int option = 0)
        {
            p.init_plain((ushort)0x101);

            p.WriteByte((byte)option);
            return 0;
        }
        public static int pacote0B4(ref packet p,
Player _session,
List<TrofelEspecialInfo> v_element,
int option = 0)
        {

            p.init_plain((ushort)0xB4);

            p.WriteInt16((short)option);

            p.WriteByte((byte)v_element.Count);

            foreach (TrofelEspecialInfo i in v_element)
            {
                p.AddBuffer(i, new TrofelEspecialInfo());
            }

            return 0;
        }

        public static int pacote0F1(ref packet p,
Player _session,
int option = 0)
        {

            p.init_plain((ushort)0xF1);

            p.WriteByte((byte)option);

            return 0;
        }


        public static int pacote169(ref packet p,
            Player _session, TrofelInfo ti,
            int option = 0)
        {

            p.init_plain((ushort)0x169);

            p.WriteByte((byte)option);

            p.AddBuffer(ti, new TrofelInfo());

            return 0;
        }
        #endregion

    }
}
