//// Arquivo Game.cs
//// Criado em 11/08/2018 as 22:21 por Acrisio
//// Definição da classe base Game

//using GameServer.GameType;
//using PangyaAPI.Utilities;
//using System.Collections.Generic;
//using System.Linq;
//using static GameServer.GameType._Define;
//using System;
//using _smp = PangyaAPI.Utilities.Log;
//using GameServer.Session;
//using PangyaAPI.Utilities.BinaryModels;
//using PangyaAPI.Utilities.Log;
//using GameServer.Game.System;
//using GameServer.Game.Manager;
//using GameServer.PangyaEnums;
//using GameServer.PacketFunc;
//using PangyaAPI.Network.PangyaPacket;
//using PangyaAPI.Network.Pangya_St;
//using PangyaAPI.SQL.Manager;
//using GameServer.Cmd;
//using System.Runtime.InteropServices;
//using PangyaAPI.IFF.JP.Models.Data;
//using PangyaAPI.SQL;
//using System.Threading;

//namespace GameServer.Game
//{
//    public abstract class Game
//    {
//        protected List<Player> m_players;
//        protected Dictionary<Player, PlayerGameInfo> m_player_info;
//        protected List<PlayerGameInfo> m_player_order;
//        protected Dictionary<uint, uint> m_player_report_game;

//        protected RoomInfoEx m_ri;
//        protected RateValue m_rv;
//        protected int m_game_init_state;
//        protected bool m_state;
//        protected DateTime m_start_time;
//        protected Timer m_timer;
//        protected byte m_channel_rookie;
//        protected volatile uint m_sync_send_init_data;
//        protected Course m_course;

//        private object m_cs = new object();// criar o codigo
//        private object m_cs_sync_finish_game = new object(); // criar o codigo               
//        public Game(List<Player> _players, RoomInfoEx _ri, RateValue _rv, byte _channel_rookie)
//        {
//            m_players = _players;
//            m_ri = _ri;
//            m_rv = _rv;
//            m_channel_rookie = _channel_rookie;
//            m_state = false;
//        }
//        public virtual void sendInitialData(Player _session) { } // criar o codigo

//        // Envia os dados iniciais para quem entra depois no Game
//        public virtual void sendInitialDataAfter(Player _session) { } // criar o codigo

//        protected Player findSessionByOID(uint _oid) { } // criar o codigo
//        protected Player findSessionByUID(uint _uid) { } // criar o codigo
//        protected Player findSessionByNickname(string _nickname) { } // criar o codigo
//        protected Player findSessionByPlayerGameInfo(PlayerGameInfo _pgi) { } // criar o codigo

//        public PlayerGameInfo getPlayerInfo(Player _session) { } // criar o codigo

//        // Se _session for diferente de null retorna todas as session, menos a que foi passada no _session
//        public List<Player> getSessions(Player _session = null) { } // criar o codigo

//        public virtual PangyaTime getTimeStart() { } // criar o codigo

//        public virtual void addPlayer(Player _session) { } // criar o codigo
//        public virtual bool deletePlayer(Player _session, int _option) { } // criar o codigo

//        // Metôdos do Game.Course.Hole
//        public virtual void requestInitHole(Player _session, Packet _packet) { } // criar o codigo
//        public virtual bool requestFinishLoadHole(Player _session, Packet _packet) { } // criar o codigo
//        public virtual void requestFinishCharIntro(Player _session, Packet _packet) { } // criar o codigo
//        public virtual void requestFinishHoleData(Player _session, Packet _packet) { } // criar o codigo

//        // Server enviou a resposta do InitShot para o cliente
//        // Esse aqui é exclusivo do VersusBase
//        public abstract void requestInitShotSended(Player _session, Packet _packet); // criar o codigo

//        public abstract void requestInitShot(Player _session, Packet _packet); // criar o codigo
//        public abstract void requestSyncShot(Player _session, Packet _packet); // criar o codigo
//        public abstract void requestInitShotArrowSeq(Player _session, Packet _packet); // criar o codigo
//        public abstract void requestShotEndData(Player _session, Packet _packet); // criar o codigo
//        public abstract RetFinishShot requestFinishShot(Player _session, Packet _packet); // criar o codigo

//        public abstract void requestChangeMira(Player _session, Packet _packet); // criar o codigo
//        public abstract void requestChangeStateBarSpace(Player _session, Packet _packet); // criar o codigo
//        public abstract void requestActivePowerShot(Player _session, Packet _packet); // criar o codigo
//        public abstract void requestChangeClub(Player _session, Packet _packet); // criar o codigo
//        public abstract void requestUseActiveItem(Player _session, Packet _packet); // criar o codigo
//        public abstract void requestChangeStateTypeing(Player _session, Packet _packet); // criar o codigo    // Escrevendo
//        public abstract void requestMoveBall(Player _session, Packet _packet); // criar o codigo
//        public abstract void requestChangeStateChatBlock(Player _session, Packet _packet); // criar o codigo
//        public abstract void requestActiveBooster(Player _session, Packet _packet); // criar o codigo
//        public abstract void requestActiveReplay(Player _session, Packet _packet); // criar o codigo
//        public abstract void requestActiveCutin(Player _session, Packet _packet); // criar o codigo

//        // Hability Item
//        public abstract void requestActiveRing(Player _session, Packet _packet); // criar o codigo
//        public abstract void requestActiveRingGround(Player _session, Packet _packet); // criar o codigo
//        public abstract void requestActiveRingPawsRainbowJP(Player _session, Packet _packet); // criar o codigo
//        public abstract void requestActiveRingPawsRingSetJP(Player _session, Packet _packet); // criar o codigo
//        public abstract void requestActiveRingPowerGagueJP(Player _session, Packet _packet); // criar o codigo
//        public abstract void requestActiveRingMiracleSignJP(Player _session, Packet _packet); // criar o codigo
//        public abstract void requestActiveWing(Player _session, Packet _packet); // criar o codigo
//        public abstract void requestActivePaws(Player _session, Packet _packet); // criar o codigo
//        public abstract void requestActiveGlove(Player _session, Packet _packet); // criar o codigo
//        public abstract void requestActiveEarcuff(Player _session, Packet _packet); // criar o codigo

//        public abstract void requestActiveAutoCommand(Player _session, Packet _packet); // criar o codigo // Auto Comando [Especial Shot Help]
//        public abstract void requestActiveAssistGreen(Player _session, Packet _packet); // criar o codigo // Olho mágico

//        // Esse Aqui só tem no VersusBase e derivados dele
//        public abstract void requestMarkerOnCourse(Player _session, Packet _packet); // criar o codigo
//        public abstract void requestLoadGamePercent(Player _session, Packet _packet); // criar o codigo
//        public abstract void requestStartTurnTime(Player _session, Packet _packet); // criar o codigo
//        public abstract void requestUnOrPause(Player _session, Packet _packet); // criar o codigo

//        // Common Command GM Change Wind Versus
//        public abstract void requestExecCCGChangeWind(Player _session, Packet _packet); // criar o codigo
//        public abstract void requestExecCCGChangeWeather(Player _session, Packet _packet); // criar o codigo

//        // Continua o versus depois que o player saiu no 3 hole pra cima e se for de 18h o game
//        public abstract void requestReplyContinue(); // criar o codigo

//        // Esse Aqui só tem no TourneyBase e derivados dele
//        public abstract bool requestUseTicketReport(Player _session, Packet _packet); // criar o codigo

//        // Apenas no Practice que ele é implementado
//        public abstract void requestChangeWindNextHoleRepeat(Player _session, Packet _packet); // criar o codigo

//        // Exclusivo do Modo Tourney
//        public abstract void requestStartAfterEnter(Thread _job); // criar o codigo
//        public abstract void requestEndAfterEnter(); // criar o codigo
//        public abstract void requestUpdateTrofel(); // criar o codigo

//        // Excluviso do Modo Match
//        public abstract void requestTeamFinishHole(Player _session, Packet _packet); // criar o codigo

//        // Game
//        public abstract bool requestFinishGame(Player _session, Packet _packet); // criar o codigo

//        // Pede o Hole que o player está, 
//        // se eles estiver jogando ou 0 se ele não está jogando
//        public virtual byte requestPlace(Player _session)
//        {
//            if (!_session.getState())
//                throw new exception("[Game::requestPlace][Error] player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE, 1, 0));

//            // Valor padrão
//            byte hole = 0;

//            INIT_PLAYER_INFO("requestPlace", "tentou pegar o lugar[Hole] do player no jogo", _session, out PlayerGameInfo pgi);

//            if (pgi.hole != 255)
//            {

//                hole = m_course.findHoleSeq(pgi.hole);

//                if (hole == 255)
//                {

//                    // Valor padrão
//                    hole = 0;

//                    _smp::message_pool.push(new message("[Game::requestPlace][Error] player[UID=" + (_session.m_pi.uid) + "] tentou pegar a sequencia do hole[NUMERO="
//                            + (pgi.hole) + "], mas ele nao encontrou no course do game na sala[NUMERO=" + (m_ri.numero) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
//                }

//            }
//            else if (pgi.init_first_hole)  // Só cria mensagem de log se o player já inicializou o primeiro hole do jogo e tem um valor inválido no pgi.hole (não é uma sequência de hole válida)
//                _smp::message_pool.push(new message("[Game::requesPlace][Error] Player[UID=" + (_session.m_pi.uid)
//                        + "] tentou pegar o hole[NUMERO=" + (pgi.hole) + "] em que o player esta na sala[NUMERO=" + (m_ri.numero)
//                        + "], mas ele esta carregando o course ou tem algum error.", type_msg.CL_FILE_LOG_AND_CONSOLE));

//            return hole;
//        } // criar o codigo

//        // Verifica se o player já esteve na sala
//        public virtual bool isGamingBefore(uint _uid)
//        {
//            if (_uid == 0)
//                throw new exception("[Game::isGamingBefore][Error] _uid is invalid(zero)", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME, 1000, 0));

//            return m_player_info.Values.Any(playerInfo => playerInfo.uid == _uid);
//        }
//        // criar o codigo

//        // Exclusivo do Modo Tourney
//        public virtual void requestSendTimeGame(Player _session) { } // criar o codigo
//        public virtual void requestUpdateEnterAfterStartedInfo(Player _session, EnterAfterStartInfo _easi) { } // criar o codigo

//        // Exclusivo do Grand Zodiac Modo
//        public virtual void requestStartFirstHoleGrandZodiac(Player _session, Packet _packet) { } // criar o codigo
//        public virtual void requestReplyInitialValueGrandZodiac(Player _session, Packet _packet) { } // criar o codigo

//        public virtual void requestReadSyncShotData(Player _session, Packet _packet, ShotSyncData _ssd) { } // criar o codigo

//        // Smart Calculator Command
//       // public virtual bool execSmartCalculatorCmd(Player _session, string _msg, eTYPE_CALCULATOR_CMD _type) { } // criar o codigo

//       // public virtual stGameShotValue getGameShotValueToSmartCalculator(Player _session, byte _club_index, byte _power_shot_index) { } // criar o codigo

//        // Tempo
//        // Start Time Tem sua definições
//        //public virtual void startTime() {  } // criar o codigo

//        public virtual bool stopTime() { } // criar o codigo

//        public virtual bool pauseTime() { } // criar o codigo
//        public virtual bool resumeTime() { } // criar o codigo

//        // time is over tem suas definições
//        //public virtual void timeIsOver() {  } // criar o codigo

//        // Report Game
//        public virtual void requestPlayerReportChatGame(Player _session, Packet _packet) { } // criar o codigo
                   
//			protected virtual void initPlayersItemRainRate() { } // criar o codigo
//        protected virtual void initPlayersItemRainPersistNextHole() { } // criar o codigo
//        protected virtual void initArtefact() { } // criar o codigo

//        protected virtual PlayerGameInfo.eCARD_WIND_FLAG getPlayerWindFlag(Player _session) { } // criar o codigo
//        protected virtual int initCardWindPlayer(PlayerGameInfo _pgi, byte _wind) { } // criar o codigo

//        protected virtual PlayerGameInfo.stTreasureHunterInfo getPlayerTreasureInfo(Player _session) { } // criar o codigo

//        protected virtual void updatePlayerAssist(Player _session) { } // criar o codigo

//        protected virtual void initGameTime() { } // criar o codigo

//        protected virtual uint getRankPlace(Player _session) { } // criar o codigo

//        protected virtual DropItemRet requestInitDrop(Player _session) { } // criar o codigo

//        protected virtual void requestSaveDrop(Player _session) { } // criar o codigo

//        protected virtual DropItemRet requestInitCubeCoin(Player _session, Packet _packet) { } // criar o codigo

//        protected virtual void requestCalculePang(Player _session) { } // criar o codigo

//        protected virtual void requestSaveInfo(Player _session, int option) { } // criar o codigo

//        protected virtual void requestUpdateItemUsedGame(Player _session) { } // criar o codigo

//        protected virtual void requestFinishItemUsedGame(Player _session) { } // criar o codigo

//        protected virtual void requestFinishHole(Player _session, int option) { } // criar o codigo

//        protected virtual void requestSaveRecordCourse(Player _session, int game, int option) { } // criar o codigo

//        protected virtual void requestInitItemUsedGame(Player _session, PlayerGameInfo _pgi) { } // criar o codigo

//        protected virtual void requestSendTreasureHunterItem(Player _session) { } // criar o codigo

//        protected virtual byte checkCharMotionItem(Player _session) { } // criar o codigo

//        // Atualiza o Info do usuario, Info Trofel e Map Statistics do Course
//        // Opt 0 Envia tudo, -1 não envia o map statistics
//        protected virtual void sendUpdateInfoAndMapStatistics(Player _session, int _option) { } // criar o codigo

//        // Envia a message no char para todos player do Game que o player terminou o jogo
//        protected virtual void sendFinishMessage(Player _session) { } // criar o codigo

//        protected virtual void requestCalculeRankPlace() { } // criar o codigo

//        // Set Flag Game and finish_game flag
//        protected virtual void setGameFlag(PlayerGameInfo _pgi, PlayerGameInfo.eFLAG_GAME _fg) { } // criar o codigo
//        protected virtual void setFinishGameFlag(PlayerGameInfo _pgi, byte _finish_game) { } // criar o codigo

//        // Check And Clear
//        protected virtual bool AllCompleteGameAndClear() { } // criar o codigo
//        protected virtual bool PlayersCompleteGameAndClear() { } // criar o codigo

//        // Verifica se é o ultimo hole feito
//        protected virtual bool checkEndGame(Player _session) { } // criar o codigo

//        // Retorna todos os player que entrou no jogo, exceto os que quitaram
//        protected virtual uint getCountPlayersGame() { } // criar o codigo

//        // Inicializa Jogo e Finaliza Jogo
//        protected virtual bool init_game() { } // criar o codigo

//        // Trata Shot Sync Data
//        protected virtual void requestTranslateSyncShotData(Player _session, ShotSyncData _ssd) { } // criar o codigo
//        protected virtual void requestReplySyncShotData(Player _session) { } // criar o codigo

//        protected virtual void clear_time() { } // criar o codigo

//        protected virtual void clear_player_order() { } // criar o codigo

//        // Achievement
//        protected virtual void initAchievement(Player _session) { } // criar o codigo
//        protected virtual void records_player_achievement(Player _session) { } // criar o codigo
//        protected virtual void update_sync_shot_achievement(Player _session, Location _last_location) { } // criar o codigo
//        protected virtual void rain_hole_consecutivos_count(Player _session) { } // criar o codigo
//        protected virtual void score_consecutivos_count(Player _session) { } // criar o codigo  // 2 holes ou mais consecutivos
//        protected virtual void rain_count(Player _session) { } // criar o codigo

//        // Effect Active in Shot Player
//        protected virtual void setEffectActiveInShot(Player _session, ulong _effect) { } // criar o codigo

//        // Limpa os dados que são usados para cada tacada, reseta ele para usar na próxima tacada
//        protected virtual void clearDataEndShot(PlayerGameInfo _pgi) { } // criar o codigo

//        protected virtual void checkEffectItemAndSet(Player _session, uint _typeid) { } // criar o codigo


//        public abstract bool finish_game(Player _session, int option = 0) { } // criar o codigo

//        protected static void SQLDBResponse(uint _msg_id, Pangya_DB _pangya_db, object _arg) { } // criar o codigo
//        protected static bool sort_player_rank(PlayerGameInfo _pgi1, PlayerGameInfo _pgi2) { } // criar o codigo

//        protected void makePlayerInfo(Player _session) { } // criar o codigo

//        protected void clearAllPlayerInfo() { } // criar o codigo

//        protected virtual void initAllPlayerInfo() { } // criar o codigo

//        // Make Object Player Info Polimofirsmo
//        protected virtual PlayerGameInfo makePlayerInfoObject(Player _session) { } // criar o codigo


//        public void DECRYPT16(ref byte[] _buffer, int _size, byte[] _key)
//        {
//            if (_size > 0 && (_buffer) != null && _key != null)
//                for (var i = 0u; i < (_size); ++i)
//                    (_buffer)[i] ^= (_key)[i % 16];
//        }
//        public void INIT_PLAYER_INFO(string _method, string _msg, Player __session, out PlayerGameInfo pgi)
//        {
//            pgi = getPlayerInfo((__session));
//            if (pgi == null)
//                throw new exception("[Game::" + ((_method)) + "][Error] player[UID=" + ((__session).m_pi.uid) + "] " + ((_msg)) + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME, 1, 4));

//        }
//    }
//}