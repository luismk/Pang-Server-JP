using GameServer.Cmd;
using GameServer.PangType;
using System;
using _smp = PangyaAPI.Utilities.Log;
using snmdb = PangyaAPI.SQL.Manager;
using packet = PangyaAPI.Network.PangyaPacket.Packet;
using PangyaAPI.SQL;
using GameServer.Session;
using PangyaAPI.Network.Pangya_St;
using GameServer.PacketFunc;
using PangyaAPI.Utilities.Log;

namespace GameServer.Game
{
    public class LoginTask
    {
        Player m_session;
        readonly KeysOfLogin m_kol;
        readonly PlayerInfo m_pi;
        readonly object m_gs;
        uint m_count;
        bool m_finish;


        public LoginTask(Player _session, KeysOfLogin _kol, PlayerInfo _pi, object _gs)
        {
            m_session = _session;
            m_kol = _kol;
            m_pi = _pi;
            m_gs = _gs;
            m_count = 0u;
            m_finish = false;
        }

        public void exec()
        {
            snmdb.NormalManagerDB.add(2, new CmdUserEquip(m_pi.uid), LoginManager.SQLDBResponse, this);
        }

        public Player getSession
        { get => m_session; set  { m_session = value; } }

        public bool UpdateSession(Player _old)
        {
            m_session = _old;
            return true; 
        }

        public KeysOfLogin getKeysOfLogin()
        {
            return m_kol;
        }

        public player_info getInfo()
        {
            return m_pi;
        }

        public void finishSessionInvalid()
        { // Terminou a Tarefa, muda o estádo do task para finish
            m_finish = true;
        }

        public void sendFailLogin()
        {
            try
            {                                                       
                packet p = new packet();
                p.Write(new byte[] { 0x44, 0x01, });
                p.WriteUInt32(1);
                packet_func.session_send(ref p, m_session, 1);     
            }
            catch (Exception)
            {
                _smp.message_pool.push("[LoginTask::sendFailLogin][ErrorSystem] ");
            }

            // Terminou a Tarefa, muda o estádo do task para finish
            m_finish = true;
        }

        public void sendCompleteData()
        {
            //// Verifica se a session ainda é valida, essas funções já é thread-safe
            if (!m_session.getConnected())
            {

                _smp.message_pool.push("[LoginTask::sendCompleteData][Error] session is invalid.");

                finishSessionInvalid();

                return;
            }

            try
            {

                packet p = new packet();

                //// Check All Character All Item Equiped is on Warehouse Item of Player
                //foreach (var el in m_session.m_pi.mp_ce)
                //{
                //    // Check Parts of Character e Check Aux Part of Character
                //    //m_session.checkCharacterAllItemEquiped(el.Value);
                //}

                // Check All Item Equiped
                //m_session.checkAllItemEquiped(m_session.m_pi.ue);


                var pi = m_session.m_pi;
                // Envia todos pacotes aqui, alguns envia antes, por que agora estou usando o jeito o pangya original
                packet_func.pacote044(ref p, m_session, Program.gs.m_si, 0, pi);
                packet_func.session_send(ref p, m_session, 1);

                packet_func.pacote070(ref p, m_session, pi.mp_ce);
                packet_func.session_send(ref p, m_session, 1);
                packet_func.pacote071(ref p, m_session, pi.mp_ci);
                packet_func.session_send(ref p, m_session, 1);
                packet_func.pacote073(ref p, m_session, pi.mp_wi);
                packet_func.session_send(ref p, m_session, 1);
                                                      
                packet_func.pacote0E1(ref p, m_session, pi.mp_mi);
                packet_func.session_send(ref p, m_session, 1);
                packet_func.pacote072(ref p, m_session, pi.ue);
                packet_func.session_send(ref p, m_session, 1);

                Program.gs.sendChannelListToSession(m_session);

                ////// // Treasure Hunter Info
                ////packet_func.pacote131(ref p);
                ////packet_func.session_send(ref p, m_session, 1);

                ////pi.mgr_achievement.sendCounterItemToPlayer(m_session);
                ////pi.mgr_achievement.sendAchievementToPlayer(m_session);

                //packet_func.pacote0F1(ref p, m_session);
                //packet_func.session_send(ref p, m_session, 1);


                //packet_func.pacote135(ref p, m_session);
                //packet_func.session_send(ref p, m_session, 1);

                //packet_func.pacote138(ref p, m_session, pi.v_card_info);
                //packet_func.session_send(ref p, m_session, 1);

                //packet_func.pacote136(ref p, m_session);
                //packet_func.session_send(ref p, m_session, 1);

                //packet_func.pacote137(ref p, m_session, pi.v_cei);
                //packet_func.session_send(ref p, m_session, 1);

                //packet_func.pacote13F(ref p, m_session);
                //packet_func.session_send(ref p, m_session, 1);

                //packet_func.pacote181(ref p, m_session, pi.v_ib);
                //packet_func.session_send(ref p, m_session, 1);

                //packet_func.pacote096(ref p, m_session, m_pi);
                //packet_func.session_send(ref p, m_session, 1);

                //packet_func.pacote169(ref p, m_session, pi.ti_current_season, 5/*season atual*/);
                //packet_func.session_send(ref p, m_session, 1);
                //packet_func.pacote169(ref p, m_session, pi.ti_rest_season);
                //packet_func.session_send(ref p, m_session, 1);

                //packet_func.pacote0B4(ref p, m_session, pi.v_tsi_current_season, 5/*season atual*/);
                //packet_func.session_send(ref p, m_session, 1);
                //packet_func.pacote0B4(ref p, m_session, pi.v_tsi_rest_season);
                //packet_func.session_send(ref p, m_session, 1);

                //packet_func.pacote158(ref p, m_session, pi.uid, pi.ui, 0);
                //packet_func.session_send(ref p, m_session, 1);    // Total de season, 5 atual season

                //packet_func.pacote25D(ref p, m_session, pi.v_tgp_current_season, 5/*season atual*/);
                //packet_func.session_send(ref p, m_session, 1);
                //packet_func.pacote25D(ref p, m_session, pi.v_tgp_rest_season, 0);
                //packet_func.session_send(ref p, m_session, 1);


                //// Login Reward System - verifica se o player ganhou algum item por logar
                //if (sgs::gs::getInstance().getInfo().rate.login_reward_event)
                //    sLoginRewardSystem::getInstance().checkRewardLoginAndSend(m_session);
            }
            catch (Exception ex)
            {
                _smp.message_pool.push(new message(
              $"[LoginTask::sendCompleteData][ErrorSystem] {ex.Message}\nStack Trace: {ex.StackTrace}",
              type_msg.CL_FILE_LOG_AND_CONSOLE));                            
            }

            // Terminou a Tarefa, muda o estádo do task para finish
            m_finish = true;
            Program.gs.m_login_manager.deleteTask(this);
        }

        public void sendReply(uint _msg_id)
        {
            packet p = new packet();
            p.Write(new byte[] { 0x44 , 0xD2});
            p.WriteUInt32(_msg_id);        
            packet_func.session_send(ref p, m_session, 1);
        }

        public uint getCount()
        {
            return m_count;
        }

        public bool isFinished()
        {
            return m_finish;
        }                   

        public void incremenetCount()
        {
            ++m_count;                   
        }   
    }
}
