using GameServer.Game;
using GameServer.Game.Manager;
using GameServer.GameType;
using PangyaAPI.Network.Pangya_St;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using _smp = PangyaAPI.Utilities.Log;
using static GameServer.GameType._Define;
using GameServer.Session;
using System.Xml.Linq;

namespace GameServer.PacketFunc
{
    /// <summary>
    /// somente responde os pacotes chamados pelo cliente
    /// </summary>
    public static class packet_func_sv
    {
        static int MAX_BUFFER_PACKET = 1000;

        #region Response Packet
        public static byte[] InitialLogin(PlayerInfo pi, ServerInfoEx _si)
        {
            var p = new PangyaBinaryWriter();
            try
            {
                if (pi == null)
                    throw new exception("Erro PlayerInfo *pi is null. packet_func_sv::InitialLogin()");

                p.WritePStr(_si.version_client);

                // member info
                p.WriteUInt16(ushort.MaxValue);//num the room
                //write struct info player      
                p.WriteBytes(pi.mi.Build());
                // User Info player
                p.WriteUInt32(pi.uid);
                p.WriteBytes(pi.ui.Build());

                // Trofel Info
                p.WriteBytes(pi.ti_current_season.Build());
                // User Equip
                p.WriteBytes(pi.ue.Build());
                #region MapStatic Work 
                //---------------------------- MAP STATISTIC -------------------------------\\
                // Map Statistics Normal
                for (byte st_i = 0; st_i < MS_NUM_MAPS; st_i++)
                {
                    p.WriteBytes(pi.a_ms_normal[st_i].Build());
                }

                // Map Statistics Natural
                for (byte st_i = 0; st_i < MS_NUM_MAPS; st_i++)
                {
                    p.WriteBytes(pi.a_ms_natural[st_i].Build());
                }

                // Map Statistics Grand Prix
                for (byte st_i = 0; st_i < MS_NUM_MAPS; st_i++)
                {
                    p.WriteBytes(pi.a_ms_grand_prix[st_i].Build());
                }

                // Map Statistics Normal for all seasons
                for (int j = 0; j < 9; j++)
                {
                    for (var st_i = 0; st_i < MS_NUM_MAPS; st_i++)        //talvez algum problema aqui!
                    {
                        p.WriteBytes(pi.aa_ms_normal_todas_season[st_i].Build());
                    }
                }
                //---------------------------- MAP STATIC CORRECT -------------------------------\\
                #endregion fim
                //Equiped Items
                p.WriteBytes(pi.ei.Build());
                // Write Time, 16 Bytes
                p.WriteTime();

                // Config do Server
                p.WriteUInt16(0); // Valor padrão, 1 na primeira vez, 2 para logins subsequentes
                p.WriteStruct(pi.mi.papel_shop, pi.mi.papel_shop);
                p.WriteInt32(0); // Valor novo no JP, indicado como 0 em novas contas
                p.WriteUInt64(pi.block_flag.m_flag.ullFlag); // Flag do server para bloquear sistemas
                p.WriteUInt32(pi.ari.counter); // Quantidade de vezes que logou
                p.WriteUInt32(_si.propriedade.ulProperty);

                //if (p.GetSize == 12800)
                //    Debug.WriteLine("InitialLogin Size Okay");
                //else
                //    Debug.WriteLine($"InitialLogin Size Bug: Correct = {12800}, Incorrect = {p.GetSize} => packet_func.InitialLogin()");

                return p.GetBytes;
            }
            catch (exception e)
            {
                _smp.message_pool.push("[packet_func_gs::InitialLogin]", e);
                return new byte[0];
            }
        }

        public static List<PangyaBinaryWriter> pacote046(List<PlayerCanalInfo> v_element, int option)
        {
            var responses = new List<PangyaBinaryWriter>();
            int elements = v_element.Count;
            int itensPorPacote = 20;

            // Divide a lista apenas se necessário
            var splitList = (elements * 200 < (MAX_BUFFER_PACKET - 100))
                ? new List<List<PlayerCanalInfo>> { v_element } // Envia tudo em um pacote
                : v_element.Select((item, index) => new { item, index })
                           .GroupBy(x => x.index / itensPorPacote)
                           .Select(g => g.Select(x => x.item).ToList())
                           .ToList();

            // Gera pacotes corretamente
            foreach (var lista in splitList)
            {
                var p = new PangyaBinaryWriter();
                p.Write(new byte[] { 0x46, 0x00 });
                p.WriteByte((byte)option);
                p.WriteByte((byte)lista.Count);

                foreach (var item in lista)
                {
                    p.WriteBytes(item.Build());
                }

                responses.Add(p);
            }

            return responses;
        }

        public static byte[] pacote11F(PlayerInfo pi, short tipo)
        {
            var p = new PangyaBinaryWriter();
            if (pi == null)
                throw new exception("Erro PlayerInfo *pi is null. packet_func_sv::pacote11F()");

            p.init_plain(0x11F);

            p.WriteInt16(tipo);

            p.WriteStruct(pi.TutoInfo, new TutorialInfo());
            return p.GetBytes;
        }

        public static byte[] pacote1A9(int ttl_milliseconds/*time to live*/, int option = 1)
        {
            var p = new PangyaBinaryWriter(0x1A9);

            p.WriteByte((byte)option);

            p.WriteInt32(ttl_milliseconds);
            return p.GetBytes;
        }

        public static byte[] pacote095(short sub_tipo, int option = 0, PlayerInfo pi = null)
        {
            var p = new PangyaBinaryWriter(0x95);

            p.WriteInt16(sub_tipo);

            if (sub_tipo == 0x102)
                p.WriteByte((byte)option);

            else if (sub_tipo == 0x111)
            {
                p.WriteInt32(option);

                if (pi == null)
                {
                    //delete p;

                    throw new exception("Erro PlayerInfo *pi is null. packet_func_sv::pacote095()");
                }

                p.WriteUInt64(pi.ui.pang);
            }
            return p.GetBytes;
        }

        public static List<PangyaBinaryWriter> pacote25D(List<TrofelEspecialInfo> v_element, int option)
        {
            var responses = new List<PangyaBinaryWriter>();
            int elements = v_element.Count;
            int itensPorPacote = 20;

            // Divide a lista apenas se necessário
            var splitList = (elements * 200 < (MAX_BUFFER_PACKET - 100))
                ? new List<List<TrofelEspecialInfo>> { v_element } // Envia tudo em um pacote
                : v_element.Select((item, index) => new { item, index })
                           .GroupBy(x => x.index / itensPorPacote)
                           .Select(g => g.Select(x => x.item).ToList())
                           .ToList();

            // Gera pacotes corretamente
            foreach (var lista in splitList)
            {
                var p = new PangyaBinaryWriter(0x25D);
                p.WriteByte((byte)option);
                p.WriteUInt32((uint)lista.Count);
                p.WriteUInt32((uint)lista.Count);

                foreach (var item in lista)
                {
                    p.WriteBytes(item.Build());
                }

                responses.Add(p);
            }

            return responses;
        }
        public static byte[] pacote156(uint _uid, UserEquip _ue, byte season)
        {
            var p = new PangyaBinaryWriter(0x156);

            p.WriteByte(season);

            p.WriteUInt32(_uid);
            p.WriteBytes(_ue.Build());
            return p.GetBytes;
        }


        public static byte[] pacote157(MemberInfoEx _mi, byte season)
        {
            var p = new PangyaBinaryWriter(0x157);

            p.WriteByte(season);

            p.WriteUInt32(_mi.uid);
            p.WriteUInt16(_mi.sala_numero);
            p.WriteBytes(_mi.Build());
            p.WriteUInt32(_mi.uid);
            p.WriteUInt32(_mi.guild_point);
            return p.GetBytes;
        }

        public static byte[] pacote158(uint _uid, UserInfoEx _ui, byte season)
        {
            var p = new PangyaBinaryWriter(0x158);

            p.WriteByte((byte)season);

            p.WriteUInt32(_uid);

            p.WriteBytes(_ui.Build());
            return p.GetBytes;
        }

        public static byte[] pacote159(uint uid, TrofelInfo ti, byte season)
        {
            var p = new PangyaBinaryWriter(0x159);
            p.WriteByte(season);
            p.WriteUInt32(uid);
            p.WriteBytes(ti.Build());
            return p.GetBytes;
        }

        public static byte[] pacote15A(uint uid, List<TrofelEspecialInfo> vTei, byte season)
        {
            var p = new PangyaBinaryWriter(0x15A);
            p.WriteByte(season);
            p.WriteUInt32(uid);
            p.WriteUInt16((ushort)vTei.Count);

            foreach (var item in vTei)
                p.WriteStruct(item, new TrofelEspecialInfo());

            return p.GetBytes;
        }

        public static byte[] pacote15B(uint uid, byte season)
        {
            var p = new PangyaBinaryWriter(0x15B);
            p.WriteByte(season);
            p.WriteUInt32(uid);
            p.WriteInt16(0); // Count desconhecido
            return p.GetBytes;
        }

        public static byte[] pacote15C(uint uid, List<MapStatisticsEx> vMs, List<MapStatisticsEx> vMsa, byte season)
        {
            var p = new PangyaBinaryWriter(0x15C);
            p.WriteByte(season);
            p.WriteUInt32(uid);
            p.WriteInt32(vMs.Count);

            foreach (var item in vMs)
                p.WriteBytes(item.Build());

            p.WriteInt32(vMsa.Count);

            foreach (var item in vMsa)
                p.WriteBytes(item.Build());

            return p.GetBytes;
        }

        public static byte[] pacote15D(uint uid, GuildInfo gi)
        {
            var p = new PangyaBinaryWriter(0x15D);
            p.WriteUInt32(uid);
            p.WriteBytes(gi.Build());
            return p.GetBytes;
        }

        public static byte[] pacote15E(uint uid, CharacterInfo ci)
        {
            var p = new PangyaBinaryWriter(0x15E);
            p.WriteUInt32(uid);
            p.WriteBytes(ci.Build());
            return p.GetBytes;
        }

        public static byte[] pacote096(PlayerInfo pi)
        {
            if (pi == null)
                throw new exception("Erro PlayerInfo *pi is null. packet_func_sv::pacote096()");
            using (var p = new PangyaBinaryWriter(0x96))
            {
                p.WriteUInt64(pi.cookie);
                return p.GetBytes;
            }
        }

        public static byte[] pacote181(List<ItemBuffEx> v_element, int option = 0)
        {
            using (var p = new PangyaBinaryWriter(0x181))
            {
                p.WriteInt32(option);

                if (option == 0)
                {
                    p.WriteByte(v_element.Count());
                    for (int i = 0; i < v_element.Count; i++)
                        p.WriteBytes(v_element[i].Build());

                }
                else if (option == 2)
                {
                    p.WriteUInt32((uint)v_element.Count);

                    for (int i = 0; i < v_element.Count; i++)
                    {
                        p.WriteUInt32(v_element[i]._typeid);
                        p.WriteBytes(v_element[i].Build());

                    }
                }
                else
                    p.WriteByte(0);

                return p.GetBytes;
            }
        }

        public static byte[] pacote13F(int option = 0)
        {
            using (var p = new PangyaBinaryWriter(0x13F))
            {
                p.WriteByte(option);
                return p.GetBytes;
            }
        }


        public static byte[] pacote136()
        {
            using (var p = new PangyaBinaryWriter(0x136))
            {
                return p.GetBytes;
            }
        }

        public static byte[] pacote137(CardEquipManager v_element)
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.init_plain(0x137);

                p.WriteUInt16((short)v_element.Count());
                foreach (var CardEquip in v_element.Values)
                    p.WriteBytes(CardEquip.Build());

                return p.GetBytes;
            }
        }
        public static byte[] pacote138(CardManager v_element, int option = 0)
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.Write(new byte[] { 0x38, 0x01 });
                p.WriteInt32(option);
                p.WriteUInt16((ushort)v_element.Count);
                foreach (var Card in v_element.Values)
                    p.WriteBytes(Card.Build());

                return p.GetBytes;
            }
        }

        public static byte[] pacote135()
        {
            using (var p = new PangyaBinaryWriter(0x135))
            {
                return p.GetBytes;
            }
        }

        public static byte[] pacote131(int option)
        {
            using (var p = new PangyaBinaryWriter(0x131))
            {


                //if (!sTreasureHunterSystem::getInstance().isLoad())
                //    sTreasureHunterSystem::getInstance().load();

                p.WriteByte(option);

                p.WriteByte(MS_NUM_MAPS);

                /*p.WriteBytes(TreasureHunterSystem::getAllCoursePoint(), sizeof(TreasureHunterInfo) * MS_NUM_MAPS);*/
                //p.WriteBytes(sTreasureHunterSystem::getInstance().getAllCoursePoint().Build());

                return p.GetBytes;
            }
        }

        public static byte[] pacote072(UserEquip ue)
        {
            var p = new PangyaBinaryWriter();

            p.Write(new byte[] { 0x72, 0x00 });
            p.WriteBytes(ue.Build());
            return p.GetBytes;
        }

        public static byte[] pacote0E1(MascotManager v_element, int option = 0)
        {
            var p = new PangyaBinaryWriter(0xE1);

            p.Write(v_element.Build());
            return p.GetBytes;
        }

        public static PangyaBinaryWriter pacote073(List<WarehouseItemEx> v_element, int option = 0)
        {
            var p = new PangyaBinaryWriter();
            try
            {
                p.Write(new byte[] { 0x73, 0x00 });
                p.WriteUInt16((short)v_element.Count);
                p.WriteUInt16((short)v_element.Count);
                foreach (var item in v_element)
                {
                    p.WriteBytes(item.Build());
                }
                return p;
            }
            catch (Exception)
            {
                return p;
            }
        }

        public static byte[] pacote071(CaddieManager v_element, int option = 0)
        {
            var p = new PangyaBinaryWriter();
            try
            {
                p.Write(new byte[] { 0x71, 0x00 });
                p.WriteInt16((short)v_element.Count);
                p.WriteInt16((short)v_element.Count);
                foreach (var char_info in v_element.Values)
                {
                    p.WriteBytes(char_info.Build(false));
                }
                return p.GetBytes;
            }
            catch (Exception)
            {
                return p.GetBytes;
            }
        }

        /// <summary>
        /// Send Packet for Info Characters(Personagens)
        /// </summary>
        /// <param name="v_element">object list</param>
        /// <param name="option">what?</param>
        /// <returns>obj using for write data</returns>
        public static byte[] pacote070(CharacterManager v_element, int option = 0)
        {
            var p = new PangyaBinaryWriter();
            try
            {
                p.Write(new byte[] { 0x70, 0x00 });
                p.WriteInt16((short)v_element.Count);
                p.WriteInt16((short)v_element.Count);
                foreach (var char_info in v_element.Values)
                {
                    p.WriteBytes(char_info.Build());
                }
                return p.GetBytes;
            }
            catch (Exception)
            {
                return p.GetBytes;
            }
        }

        /// <summary>
        /// packet 9D use channel list!
        /// </summary>
        /// <param name="v_element"></param>
        /// <param name="build_s">true is server, false is chanell call!</param>
        /// <returns></returns>
        public static byte[] pacote04D(List<Channel> v_element, bool build_s = false)
        {
            try
            {
                using (var p = new PangyaBinaryWriter())
                {
                    if (!build_s)
                        p.Write(new byte[] { 0x4D, 0x00 }); //channel list!         

                    p.WriteByte(v_element.Count);
                    foreach (var channel in v_element)
                        p.WriteBytes(channel.Build());

                    return p.GetBytes;
                }
            }
            catch (exception ex)
            {
                _smp.message_pool.push(new message(
              $"[packet_func_sv::pacote04D][ErrorSystem] {ex.getFullMessageError()}",
              type_msg.CL_FILE_LOG_AND_CONSOLE));
                return new byte[] { 0x4D, 0x00, 0x00 };
            }
        }

        public static byte[] pacote248(
            AttendanceRewardInfo ari,
            int option = 0)
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.Write(new byte[] { 0x48, 0x02 });
                p.WriteInt32(option);
                p.WriteBytes(ari.Build());
                return p.GetBytes;
            }
        }

        public static byte[] pacote249(
            AttendanceRewardInfo ari,
            int option = 0)
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.Write(new byte[] { 0x49, 0x02 });
                p.WriteInt32(option);
                p.WriteBytes(ari.Build());
                return p.GetBytes;
            }
        }

        public static byte[] pacote257(uint _uid, List<TrofelEspecialInfo> v_tegi, byte season)
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.init_plain(0x257);

                p.WriteByte(season);
                p.WriteUInt32(_uid);

                p.WriteInt16((short)v_tegi.Count);
                foreach (var item in v_tegi)
                    p.WriteStruct(item, new TrofelEspecialInfo());
                return p.GetBytes;
            }
        }

        public static byte[] pacote04E(int option, int _codeErrorInfo = 0)
        {
            /* Option Values
                * 1 Sucesso
                * 2 Channel Full
                * 3 Nao encontrou canal
                * 4 Nao conseguiu pegar informções do canal
                * 6 ErrorCode Info
                */
            using (var p = new PangyaBinaryWriter(0x4E))
            {
                p.WriteByte((byte)option);

                if (_codeErrorInfo != 0)
                    p.WriteInt32(_codeErrorInfo);
                return p.GetBytes;
            }
        }


        public static byte[] pacote040(string nick, string msg, byte option)
        {

            if ((option == 0 || option == 0x80) && string.IsNullOrEmpty(nick))
                throw new exception("Error PlayerInfo *pi is null. packet_func_sv::pacote040()");

            using (var p = new PangyaBinaryWriter(0x40))
            {
                p.WriteByte(option);

                if (option == 0 || option == 0x80 || option == 4)
                {
                    p.WritePStr(nick);
                    if (option != 4)
                        p.WritePStr(msg);
                }
                return p.GetBytes;
            }
        }

        public static byte[] pacote044(ServerInfoEx _si, int option, PlayerInfo pi = null, int valor = 0)
        {
            var p = new PangyaBinaryWriter(0x44);

            if (option == 0 && pi == null)
                throw new exception("Erro PlayerInfo *pi is null. packet_func_sv::pacote044()");

            p.WriteByte(option);   // Option

            if (option == 0)
                p.Write(InitialLogin(pi, _si));
            else if (option == 1)
                p.WriteByte(0);
            else if (option == 0xD3)
                p.WriteByte(0);
            else if (option == 0xD2)
                p.WriteInt32(valor);

            return p.GetBytes;
        }

        public static byte[] pacote0B2(

List<MsgOffInfo> v_element,
int option = 0)
        {
            var p = new PangyaBinaryWriter();

            p.init_plain((ushort)0xB2);

            p.WriteInt32(2); // Não sei bem o que é, mas pode ser uma opção

            p.WriteInt32(option);

            p.WriteUInt32((uint)v_element.Count);

            foreach (MsgOffInfo i in v_element)
            {
                p.WriteStruct(i, new MsgOffInfo());
            }

            return p.GetBytes;
        }
        public static byte[] pacote0D4(CaddieManager v_element)
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.init_plain(0xD4);
                p.WriteUInt32((uint)v_element.Count());
                foreach (var item in v_element.Values)
                    p.WriteBytes(item.Build());

                return p.GetBytes;
            }
        }

        // Metôdos de auxílio de criação de pacotes


        public static byte[] pacote210(

                List<MailBox> v_element,
                int option = 0)
        {
            var p = new PangyaBinaryWriter();

            p.init_plain((ushort)0x210);

            p.WriteInt32(option);

            p.WriteInt32(v_element.Count);

            for (var i = 0; i < v_element.Count; ++i)
            {
                p.WriteBytes(v_element[i].Build());
            }

            return p.GetBytes;
        }

        public static byte[] pacote10E(Last5PlayersGame l5pg)
        {
            var p = new PangyaBinaryWriter(0x10E);
            foreach (var p_log in l5pg.players)
            {
                p.WriteBytes(p_log.Build());
            }
            return p.GetBytes;
        }
        public static byte[] pacote0FC(List<ServerInfo> v_si)
        {
            var p = new PangyaBinaryWriter(0xFC);
            p.WriteByte((byte)v_si.Count);

            foreach (ServerInfo i in v_si)
                p.WriteBytes(i.Build());

            return p.GetBytes;
        }



        internal static byte[] pacote101(int option = 0)
        {
            var p = new PangyaBinaryWriter();

            p.init_plain((ushort)0x101);

            p.WriteByte((byte)option);
            return p.GetBytes;
        }
        public static byte[] pacote0B4(

List<TrofelEspecialInfo> v_element,
int option = 0)
        {
            var p = new PangyaBinaryWriter();

            p.init_plain((ushort)0xB4);

            p.WriteInt16((short)option);

            p.WriteByte((byte)v_element.Count);

            foreach (TrofelEspecialInfo i in v_element)
            {
                p.WriteStruct(i, new TrofelEspecialInfo());
            }

            return p.GetBytes;
        }

        public static byte[] pacote0F1(int option = 0)
        {
            var p = new PangyaBinaryWriter();

            p.init_plain((ushort)0xF1);

            p.WriteByte((byte)option);

            return p.GetBytes;
        }


        public static byte[] pacote169(
           TrofelInfo ti,
            int option = 0)
        {
            var p = new PangyaBinaryWriter();
            p.init_plain((ushort)0x169);

            p.WriteByte((byte)option);

            p.WriteBytes(ti.Build());

            return p.GetBytes;
        }

        public static byte[] pacote09F(List<ServerInfo> v_server, List<Channel> v_channel)
        {
            using (var p = new PangyaBinaryWriter((ushort)0x9F))
            {
                p.WriteByte((byte)v_server.Count);

                for (var i = 0; i < v_server.Count; ++i)
                {
                    p.WriteBytes(v_server[i].Build());
                }
                p.WriteBytes(pacote04D(v_channel, true));
                return p.GetBytes;
            }
        }

        public static byte[] pacote089(uint _uid = 0, byte season = 0, uint err_code = 1)
        {

            using (var p = new PangyaBinaryWriter((ushort)0x89))
            {
                p.WriteUInt32(err_code);
                if (err_code > 0)
                {
                    p.WriteByte(season);
                    p.WriteUInt32(_uid);
                }
                return p.GetBytes;
            }
        }

        public static byte[] pacote211(List<MailBox> v_element, uint pagina, uint paginas, uint error = 0)
        {

            using (var p = new PangyaBinaryWriter(0x211))
            {
                p.WriteUInt32(error);

                if (error == 0)
                {
                    p.WriteUInt32(pagina);
                    p.WriteUInt32(paginas);
                    p.WriteInt32(v_element.Count);

                    for (int i = 0; i < v_element.Count; ++i)
                    {
                        p.WriteBytes(v_element[i].Build());
                    }
                }

                return p.GetBytes;
            }
        }

        public static byte[] pacote212(EmailInfo ei, uint error = 0)
        {

            using (var p = new PangyaBinaryWriter(0x212))
            {
                p.WriteUInt32(error);

                if (error == 0)
                {
                    p.WriteBytes(ei.Build());
                }

                return p.GetBytes;
            }
        }


        public static byte[] pacote06B(PlayerInfo pi, byte type, int err_code = 4)
        {

            if (pi == null)
            {
                throw new exception("Erro PlayerInfo *pi is null. packet_func_sv::pacote06B()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                    1, 0));
            }
            var p = new PangyaBinaryWriter(0x6B);

            p.WriteByte(err_code); // Error Code, 4 Sucesso, diferente é erro
            p.WriteByte(type);

            if (err_code == 4)
            {
                switch (type)
                {
                    case 0: // Character Equipado Com os Parts Equipado
                        if (pi.ei.char_info != null)
                        {
                            p.WriteBytes(pi.ei.char_info.Build());//, sizeof(CharacterInfo));
                        }
                        else
                        {
                            p.WriteZero(513);
                        }
                        break;
                    case 1: // Caddie Equipado
                        if (pi.ei.cad_info != null)
                        {
                            p.WriteUInt32(pi.ei.cad_info.id);
                        }
                        else
                        {
                            p.WriteZero(4);
                        }
                        break;
                    case 2: // Itens Equipáveis
                        p.WriteUInt32(pi.ue.item_slot);//, sizeof(pi.ue.item_slot));
                        break;
                    case 3: // Ball e Clubset Equipado
                        if (pi.ei.comet != null) // Ball
                        {
                            p.WriteUInt32(pi.ei.comet._typeid);
                        }
                        else
                        {
                            p.WriteZero(4);
                        }
                        p.WriteUInt32(pi.ei.csi.id); // ClubSet ID
                        break;
                    case 4: // Skins
                        p.WriteUInt32(pi.ue.skin_typeid);//, sizeof(pi.ue.skin_typeid));
                        break;
                    case 5: // Only Chracter Equipado
                        if (pi.ei.char_info != null)
                        {
                            p.WriteUInt32(pi.ei.char_info.id);
                        }
                        else
                        {
                            p.WriteZero(4);
                        }
                        break;
                    case 8: // Mascot Equipado
                        if (pi.ei.mascot_info != null)
                        {
                            p.WriteBytes(pi.ei.mascot_info.Build());//, sizeof(MascotInfo));
                        }
                        else
                        {
                            p.WriteZero(62);
                        }
                        break;
                    case 9: // Character Cutin Equipado
                        if (pi.ei.char_info != null)
                        {
                            p.WriteUInt32(pi.ei.char_info.id);
                            p.WriteUInt32(pi.ei.char_info.cut_in);//, sizeof(pi.ei.char_info.cut_in));
                        }
                        else
                        {
                            p.WriteZero(20);
                        }
                        break;
                    case 10: // Poster Equipado
                        p.WriteUInt32(pi.ue.poster);//, sizeof(pi.ue.poster));
                        break;
                }
            }

            return p.GetBytes;
        }

        public static byte[] pacote1D4(string _AuthKeyLogin, int option = 0)
        {
            using (var p = new PangyaBinaryWriter(0x1D4))
            {
                p.WriteInt32(option);

                if (option == 0 && !string.IsNullOrEmpty(_AuthKeyLogin))
                    p.WritePStr(_AuthKeyLogin);

                return p.GetBytes;
            }
        }
        public static byte[] pacote04B(Player _session, byte _type,
         int error = 0, int _valor = 0)
        {

            var p = new PangyaBinaryWriter(0x4B);
            if (_session == null)
            {
                throw new exception("Error _session is nullptr. Em packet_func_sv::pacote04B()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                    1, 0));
            }

            if (!_session.GetState())
            {
                throw new exception("Error player nao esta mais connectado. Em packet_func_sv::pacote04B()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                    2, 0));
            }
            p.WriteInt32(error);

            if (error == 0)
            {
                p.WriteByte(_type);

                p.WriteUInt32(_session.m_oid);

                switch (_type)
                {
                    case 1: // Caddie
                        if (_session.m_pi.ei.cad_info != null)
                        {
                            p.WriteBytes(_session.m_pi.ei.cad_info.Build());
                        }
                        else
                        {
                            p.WriteZero(25);
                        }
                        break;
                    case 2: // Ball(Comet)
                        if (_session.m_pi.ei.comet != null)
                        {
                            p.WriteUInt32(_session.m_pi.ei.comet._typeid);
                        }
                        else
                        {
                            p.WriteZero(4);
                        }
                        break;
                    case 3: // ClubSet
                        p.WriteBytes(_session.m_pi.ei.csi.Build());
                        break;
                    case 4: // Character
                        if (_session.m_pi.ei.char_info != null)
                        {
                            p.WriteBytes(_session.m_pi.ei.char_info.Build());
                        }
                        else
                        {
                            p.WriteZero(513);
                        }
                        break;
                    case 5: // Mascot
                        if (_session.m_pi.ei.mascot_info != null)
                        {
                            p.WriteBytes(_session.m_pi.ei.mascot_info.Build());
                        }
                        else
                        {
                            p.WriteZero(62);
                        }
                        break;
                    case 6: // Itens Active 1 = Jester big cabeça, 2 = Hermes velocidade x2, 3 = Twilight Fogos na cabeça
                        {
                            p.WriteInt32(_valor);

                            //if (_valor == (int)ChangePlayerItemRoom.stItemEffectLounge.TYPE_EFFECT.TE_TWILIGHT)
                            //{
                            //    p.WriteInt32(1); // Ativa Fogos
                            //}
                            //else
                            //{

                            //    if (_session.m_pi.ei.char_info != null)
                            //    {
                            //        var it = (_session.m_pi.ei.char_info == null) ? _session.m_pi.mp_scl.end() : _session.m_pi.mp_scl.find(_session.m_pi.ei.char_info.id);

                            //        if (it == _session.m_pi.mp_scl.end())
                            //        {

                            //            _smp.message_pool.getInstance().push(new message("[channel::pacote04B][Error] player[UID=" + Convert.ToString(_session.m_pi.m_uid) + "] nao tem os estados do character na lounge. Criando um novo para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                            //            // Add New State Character Lounge
                            //            var pair = _session.m_pi.mp_scl.insert(Tuple.Create(_session.m_pi.ei.char_info.id, new StateCharacterLounge({ })));

                            //            it = pair.first;
                            //        }

                            //        switch (_valor)
                            //        {
                            //            case ChangePlayerItemRoom.stItemEffectLounge.TYPE_EFFECT.TE_BIG_HEAD: // Jester (Big head)
                            //                p.addFloat(it.second.scale_head);
                            //                break;
                            //            case ChangePlayerItemRoom.stItemEffectLounge.TYPE_EFFECT.TE_FAST_WALK: // Hermes (Velocidade x2)
                            //                p.addFloat(it.second.walk_speed);
                            //                break;
                            //        }
                            //    }
                        }
                        break;
                    case 7: // Player game
                            // Nada Aqui
                        break;
                    default:
                        throw new exception("Error tipo desconhecido. Em packet_func_sv::pacote04B()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                            3, 0));
                }
            }

            return p.GetBytes;
        }

        public static byte[] pacote1AD(string webKey, int option)
        {
            using (var p = new PangyaBinaryWriter(0x1AD))
            {
                p.WriteInt32(option);

                if (webKey.empty())
                    p.WriteInt16(0);
                else
                    p.WritePStr(webKey);

                return p.GetBytes;
            }
        }

        public static byte[] pacote102(PlayerInfo pi)
        {
            if (pi == null)
            {
                throw new exception("[packet_func_sv::pacote12][Error] PlayerInfo *pi is nullptr.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                    1, 0));
            }
            using (var p = new PangyaBinaryWriter(0x102))
            {
                p.WriteUInt32(pi.cg.normal_ticket);
                p.WriteUInt32(pi.cg.partial_ticket);

                p.WriteUInt64(pi.ui.pang);
                p.WriteUInt64(pi.cookie);


                return p.GetBytes;
            }
        }

        public static byte[] pacote144(int option = 0)
        {
            var p = new PangyaBinaryWriter(0x144);
            p.WriteByte((byte)option);

            return p.GetBytes;
        }

        public static byte[] pacote09A(int ulCapability)
        {        // UPDATE ON GAME
            var p = new PangyaBinaryWriter(0x9A);

            p.WriteInt32(ulCapability);
            return p.GetBytes;
        }

        //not tested
        public static List<PangyaBinaryWriter> pacote048(Player _session, List<PlayerRoomInfoEx> v_element, int option = 0)
        {
            var p = new PangyaBinaryWriter();
            var p_list = new List<PangyaBinaryWriter>();
            if ((option & 0xFF) == 2)
            { // exit player
                p.init_plain(0x48);
                p.WriteByte((byte)option);
                p.WriteInt16(-1);

                p.WriteUInt32(_session.m_oid);
                p_list.Add(p);
                return p_list;
            }
            else if ((option & 0xFF) == 7)
            {
                var elements = v_element.Count;
                var splitList = v_element.ToList().Split(20); //ChunkBy(this.ToList(), totalBySplit);

                //Percorre lista e adiciona ao resultado
                foreach (var players in splitList)
                {
                    p.init_plain(0x48);
                    p.WriteByte((byte)option);
                    p.WriteInt16(-1);
                    if ((option & 0xFF) == 0 || (option & 0xFF) == 5)
                    {
                        p.WriteByte((byte)players.Count);
                    }
                    else if ((option & 0xFF) == 7)
                    {
                        p.WriteByte((byte)elements);
                    }
                    else if ((option & 0xFF) == 3)
                    {
                        p.WriteUInt32(_session.m_oid);
                    }
                    foreach (var info in players)
                    {
                        p.WriteBytes(info.BuildEx());
                    }

                    p.WriteByte(0); // Final list de PlayerRoomInfo
                    p_list.Add(p);
                }
            }
            else
            {
                var elements = v_element.Count;
                if (elements * (Convert.ToBoolean(option & 0x100) ? new PlayerRoomInfo().Build().Length : new PlayerRoomInfoEx().Build().Length) < (MAX_BUFFER_PACKET - 100))
                {
                    var splitList = v_element.ToList().Split(20); //ChunkBy(this.ToList(), totalBySplit);

                    //Percorre lista e adiciona ao resultado
                    foreach (var players in splitList)
                    {
                        p.init_plain(0x48);
                        p.WriteByte((byte)option);
                        p.WriteInt16(-1);
                        if ((option & 0xFF) == 0 || (option & 0xFF) == 5)
                        {
                            p.WriteByte((byte)elements);
                        }
                        else if ((option & 0xFF) == 3)
                        {
                            p.WriteUInt32(_session.m_oid);
                        }

                        for (var i = 0; i < players.Count; i++)
                        {
                            p.WriteBytes((option & 0x100) != 0 ? players[i].Build() : players[i].BuildEx());
                        }
                        p.WriteByte(0); // Final list de PlayerRoomInfo
                        p_list.Add(p);
                    }
                }
                else
                {
                    var splitList = v_element.ToList().Split(20); //ChunkBy(this.ToList(), totalBySplit);
                    uint index = 0;
                    //Percorre lista e adiciona ao resultado
                    foreach (var players in splitList)
                    {
                        p.init_plain(0x48);
                        p.WriteByte((byte)option);
                        p.WriteInt16(-1);

                        if ((option & 0xFF) == 0 && index != 0)//ver depois
                        {
                            p.WriteByte(5); // Option 5 é para add os players aos que já tem na sala
                        }
                        else
                        {
                            p.WriteByte((byte)option);
                        }

                        p.WriteInt16(-1);

                        if ((option & 0xFF) == 0 || (option & 0xFF) == 5)
                        {
                            p.WriteByte((byte)players.Count);
                        }
                        else if ((option & 0xFF) == 3)
                        {
                            p.WriteUInt32(_session.m_oid);
                        }
                        for (var i = 0; i < players.Count; i++)
                            p.WriteBytes((option & 0x100) != 0 ? players[i].Build() : players[i].BuildEx());


                        p.WriteByte(0); // Final list de PlayerRoomInfo
                        p_list.Add(p);
                        index++;
                    }
                }
            }

            return p_list;
        }


        // Metôdos de auxílio de criação de pacotes 
        public static void channel_broadcast(Channel _channel,
            PangyaBinaryWriter p, byte _debug)
        {

            List<Player> channel_session = _channel.getSessions();

            for (var i = 0; i < channel_session.Count; ++i)
            {

                var mb = (p).GetBytes;
                try
                {

                    (channel_session[i]).Send(mb);
                    if ((channel_session[i]).Devolve())
                    {
                        sgs.gs.getInstance().DisconnectSession((channel_session[i]));
                    }
                }
                catch (exception e)
                {
                    if (!ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
                        STDA_ERROR_TYPE.SESSION, 6))
                    {
                        if ((channel_session[i]).Devolve())
                        {
                            sgs.gs.getInstance().DisconnectSession((channel_session[i]));
                        }
                    }
                    if (ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
                        STDA_ERROR_TYPE.SESSION, 2))
                    {
                        throw;
                    }
                };

            }

        }

        public static void channel_broadcast(Channel _channel,
            List<PangyaBinaryWriter> v_p,
            byte _debug)
        {

            List<Player> channel_session = new List<Player>();

            for (var i = 0; i < v_p.Count; ++i)
            {
                if (v_p[i] != null)
                {
                    channel_session = _channel.getSessions();

                    for (var ii = 0; ii < channel_session.Count; ++ii)
                    {
                        var mb = (v_p[i]).GetBytes;
                        try
                        {

                            (channel_session[ii]).Send(mb);
                            if ((channel_session[ii]).Devolve())
                            {
                                sgs.gs.getInstance().DisconnectSession((channel_session[ii]));
                            }
                        }
                        catch (exception e)
                        {
                            if (!ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
                                STDA_ERROR_TYPE.SESSION, 6))
                            {
                                if ((channel_session[ii]).Devolve())
                                {
                                    sgs.gs.getInstance().DisconnectSession((channel_session[ii]));
                                }
                            }
                            if (ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
                                STDA_ERROR_TYPE.SESSION, 2))
                            {
                                throw;
                            }
                        };
                    }

                    v_p[i] = null;
                }
                else
                {
                    _smp.message_pool.push(new message("Error PangyaBinaryWriter p is nullptr, PangyaBinaryWriter_func::channel_broadcast()", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
            }

            v_p.Clear();
        }

        public static void lobby_broadcast(Channel _channel,
            PangyaBinaryWriter p, byte _debug)
        {

            List<Player> channel_session = _channel.getSessions();

            for (var i = 0; i < channel_session.Count; ++i)
            {
                if (channel_session[i].m_pi.mi.sala_numero == ushort.MaxValue)
                { // Apenas quem está na lobby, sem ser em uma sala

                    var mb = (p).GetBytes;
                    try
                    {

                        (channel_session[i]).Send(mb);
                        if ((channel_session[i]).Devolve())
                        {
                            sgs.gs.getInstance().DisconnectSession((channel_session[i]));
                        }
                    }
                    catch (exception e)
                    {
                        if (!ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
                            STDA_ERROR_TYPE.SESSION, 6))
                        {
                            if ((channel_session[i]).Devolve())
                            {
                                sgs.gs.getInstance().DisconnectSession((channel_session[i]));
                            }
                        }
                        if (ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
                            STDA_ERROR_TYPE.SESSION, 2))
                        {
                            throw;
                        }
                    };

                }
            }

        }
        //public static void room_broadcast(room _room,
        //    byte[] p, byte _debug)
        //{

        //    List<Player> room_session = _room.getSessions(null, false);

        //    for (var i = 0; i < room_session.Count; ++i)
        //    {
        //        var mb = (p);
        //        try
        //        {

        //            (room_session[i]).Send(mb);
        //            if ((room_session[i]).Devolve())
        //            {
        //                sgs.gs.getInstance().DisconnectSession((room_session[i]));
        //            }
        //        }
        //        catch (exception e)
        //        {
        //            if (!ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
        //                STDA_ERROR_TYPE.SESSION, 6))
        //            {
        //                if ((room_session[i]).Devolve())
        //                {
        //                    sgs.gs.getInstance().DisconnectSession((room_session[i]));
        //                }
        //            }
        //            if (ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
        //                STDA_ERROR_TYPE.SESSION, 2))
        //            {
        //                throw;
        //            }
        //        };

        //    }
        //}
        //public static void room_broadcast(room _room,
        //    PangyaBinaryWriter p, byte _debug)
        //{

        //    List<Player> room_session = _room.getSessions(null, false);

        //    for (var i = 0; i < room_session.Count; ++i)
        //    {
        //        var mb = (p).GetBytes;
        //        try
        //        {

        //            (room_session[i]).Send(mb);
        //            if ((room_session[i]).Devolve())
        //            {
        //                sgs.gs.getInstance().DisconnectSession((room_session[i]));
        //            }
        //        }
        //        catch (exception e)
        //        {
        //            if (!ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
        //                STDA_ERROR_TYPE.SESSION, 6))
        //            {
        //                if ((room_session[i]).Devolve())
        //                {
        //                    sgs.gs.getInstance().DisconnectSession((room_session[i]));
        //                }
        //            }
        //            if (ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
        //                STDA_ERROR_TYPE.SESSION, 2))
        //            {
        //                throw;
        //            }
        //        };

        //    }
        //}

        //public static void room_broadcast(room _room,
        //    List<PangyaBinaryWriter> v_p,
        //    byte _debug)
        //{
        //    List<Player> room_session = new List<Player>();

        //    for (var i = 0; i < v_p.Count; ++i)
        //    {
        //        if (v_p[i] != null)
        //        {
        //            room_session = _room.getSessions(null, false);

        //            for (var ii = 0; ii < room_session.Count; ++ii)
        //            {
        //                var mb = (v_p[i]).GetBytes;
        //                try
        //                {
        //                    (room_session[ii]).Send(mb);
        //                    if ((room_session[ii]).Devolve())
        //                    {
        //                        sgs.gs.getInstance().DisconnectSession((room_session[ii]));
        //                    }
        //                }
        //                catch (exception e)
        //                {
        //                    if (!ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
        //                        STDA_ERROR_TYPE.SESSION, 6))
        //                    {
        //                        if ((room_session[ii]).Devolve())
        //                        {
        //                            sgs.gs.getInstance().DisconnectSession((room_session[ii]));
        //                        }
        //                    }
        //                    if (ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
        //                        STDA_ERROR_TYPE.SESSION, 2))
        //                    {
        //                        throw;
        //                    }
        //                };
        //            }

        //            v_p[i] = null;
        //        }
        //        else
        //        {
        //            _smp.message_pool.push(new message("Error PangyaBinaryWriter p is nullptr, PangyaBinaryWriter_func::room_broadcast()", type_msg.CL_FILE_LOG_AND_CONSOLE));
        //        }
        //    }

        //    v_p.Clear();
        //}

        //public static void game_broadcast(Game _game,
        //    PangyaBinaryWriter p, byte _debug)
        //{

        //    var game_session = _game.getSessions();

        //    for (var i = 0; i < game_session.size(); ++i)
        //    {
        //        var mb = (p).GetBytes;
        //        try
        //        {
        //            (game_session[i]).Send(mb);
        //            if ((game_session[i]).Devolve())
        //            {
        //                sgs.gs.getInstance().DisconnectSession((game_session[i]));
        //            }
        //        }
        //        catch (exception e)
        //        {
        //            if (!ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
        //                STDA_ERROR_TYPE.SESSION, 6))
        //            {
        //                if ((game_session[i]).Devolve())
        //                {
        //                    sgs.gs.getInstance().DisconnectSession((game_session[i]));
        //                }
        //            }
        //            if (ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
        //                STDA_ERROR_TYPE.SESSION, 2))
        //            {
        //                throw;
        //            }
        //        };
        //    }
        //}

        //public static void game_broadcast(Game _game,
        //    List<PangyaBinaryWriter> v_p,
        //    byte _debug)
        //{

        //    List<Player> game_session = new List<Player>();

        //    for (var i = 0; i < v_p.Count; ++i)
        //    {
        //        if (v_p[i] != null)
        //        {
        //            game_session = _game.getSessions();

        //            for (var ii = 0; ii < game_session.Count; ++ii)
        //            {
        //                var mb = (v_p[i]).GetBytes;
        //                try
        //                {
        //                    (game_session[ii]).Send(mb);
        //                    if ((game_session[ii]).Devolve())
        //                    {
        //                        sgs.gs.getInstance().DisconnectSession((game_session[ii]));
        //                    }
        //                }
        //                catch (exception e)
        //                {
        //                    if (!ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
        //                        STDA_ERROR_TYPE.SESSION, 6))
        //                    {
        //                        if ((game_session[ii]).Devolve())
        //                        {
        //                            sgs.gs.getInstance().DisconnectSession((game_session[ii]));
        //                        }
        //                    }
        //                    if (ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
        //                        STDA_ERROR_TYPE.SESSION, 2))
        //                    {
        //                        throw;
        //                    }
        //                };
        //            }

        //            v_p[i] = null;
        //        }
        //        else
        //        {
        //            _smp.message_pool.push(new message("Error PangyaBinaryWriter p is nullptr, PangyaBinaryWriter_func::room_broadcast()", type_msg.CL_FILE_LOG_AND_CONSOLE));
        //        }
        //    }

        //    v_p.Clear();
        //}

        public static void vector_send(PangyaBinaryWriter _p,
            List<Player> _v_s,
            byte _debug)
        {

            foreach (var el in _v_s)
            {
                var mb = (_p).GetBytes;
                try
                {
                    (el).Send(mb);
                    if ((el).Devolve())
                    {
                        sgs.gs.getInstance().DisconnectSession((el));
                    }
                }
                catch (exception e)
                {
                    if (!ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
                        STDA_ERROR_TYPE.SESSION, 6))
                    {
                        if ((el).Devolve())
                        {
                            sgs.gs.getInstance().DisconnectSession((el));
                        }
                    }
                    if (ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
                        STDA_ERROR_TYPE.SESSION, 2))
                    {
                        throw;
                    }
                };

            }
        }

        public static void vector_send(List<PangyaBinaryWriter> _v_p,
            List<Player> _v_s,
            byte _debug)
        {

            foreach (var el in _v_p)
            {
                if (el != null)
                {
                    foreach (var el2 in _v_s)
                    {
                        var mb = el.GetBytes;
                        try
                        {

                            (el2).Send(mb);
                            if ((el2).Devolve())
                            {
                                sgs.gs.getInstance().DisconnectSession((el2));
                            }
                        }
                        catch (exception e)
                        {
                            if (!ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
                                STDA_ERROR_TYPE.SESSION, 6))
                            {
                                if ((el2).Devolve())
                                {
                                    sgs.gs.getInstance().DisconnectSession((el2));
                                }
                            }
                            if (ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
                                STDA_ERROR_TYPE.SESSION, 2))
                            {
                                throw;
                            }
                        };

                    }

                }
                else
                {
                    _smp.message_pool.push(new message("Error PangyaBinaryWriter p is nullptr, PangyaBinaryWriter_func::room_broadcast()", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
            }

            _v_p.Clear();
        }

        public static void session_send(PangyaBinaryWriter p,
            Player s, byte _debug)
        {

            if (s == null)
            {
                throw new exception("Error session s is nullptr, PangyaBinaryWriter_func::session_send()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                    1, 2));
            }

            var mb = (p).GetBytes;
            try
            {

                s.Send(mb);
                if ((s).Devolve())
                {
                    sgs.gs.getInstance().DisconnectSession((s));
                }
            }
            catch (exception e)
            {
                if (!ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
                    STDA_ERROR_TYPE.SESSION, 6))
                {
                    if ((s).Devolve())
                    {
                        sgs.gs.getInstance().DisconnectSession((s));
                    }
                }
                if (ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
                    STDA_ERROR_TYPE.SESSION, 2))
                {
                    throw;
                }
            }
        }

        public static void session_send(List<PangyaBinaryWriter> v_p,
            Player s, byte _debug)
        {

            if (s == null)
            {
                throw new exception("Error session s is nullptr, PangyaBinaryWriter_func::session_send()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV,
                    1, 2));
            }

            for (var i = 0; i < v_p.Count; ++i)
            {
                if (v_p[i] != null)
                {

                    var mb = (v_p[i]).GetBytes;
                    try
                    {

                        s.Send(mb);
                        if (!(s).Devolve())
                        {
                            sgs.gs.getInstance().DisconnectSession((s));
                        }
                    }
                    catch (exception e)
                    {
                        if (!ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
                            STDA_ERROR_TYPE.SESSION, 6))
                        {
                            if ((s).Devolve())
                            {
                                sgs.gs.getInstance().DisconnectSession((s));
                            }
                        }
                        if (ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
                            STDA_ERROR_TYPE.SESSION, 2))
                        {
                            throw;
                        }
                    };
                    v_p[i] = null;
                }
                else
                {
                    _smp.message_pool.push(new message("Error PangyaBinaryWriter p is nullptr, PangyaBinaryWriter_func::session_send()", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
            }

            v_p.Clear();
        }

        public static List<PangyaBinaryWriter> MakeBeginSplitPacket(ushort packet_id, int element_size, int max_packet, int elements)
        {
            var packets = new List<PangyaBinaryWriter>();
            int por_packet = ((max_packet - 100) > element_size) ? (max_packet - 100) / element_size : 1;
            int total = elements;   
            for (int index = 0; index < elements; total -= por_packet)
            {
                PangyaBinaryWriter p = new PangyaBinaryWriter();
                p.init_plain(packet_id);
                packets.Add(p);
            }
            return packets;
        }

        public static void MakeMedSplitPacket(ref PangyaBinaryWriter p, int total, int porPacket, int tipo)
        {
            if (tipo == 0)
            {
                p.WriteInt16((short)total);
                p.WriteInt16((short)((total > porPacket) ? porPacket : total));
            }
            else
            {
                p.WriteUInt32((uint)total);
                p.WriteUInt32((uint)((total > porPacket) ? porPacket : total));
            }
        }

        public static void MakeMidSplitPacketVector(ref PangyaBinaryWriter p, List<byte[]> elements, int elementSize, ref int index, int porPacket)
        {
            for (int i = 0; i < porPacket && index < elements.Count; i++, index++)
            {
                p.WriteBytes(elements[index]);
            }
        }

        public static void MakeMidSplitPacketMap(ref PangyaBinaryWriter p, Dictionary<int, byte[]> elements, int elementSize, ref int index, int porPacket)
        {
            var enumerator = elements.Values.GetEnumerator();
            for (int i = 0; i < porPacket && index < elements.Count; i++, index++)
            {
                if (enumerator.MoveNext())
                    p.WriteBytes(enumerator.Current);
            }
        }     
        #endregion
    }
}
