using GameServer.Cmd;
using GameServer.PangType;
using PangyaAPI.SQL;
using PangyaAPI.SQL.Manager;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using _smp = PangyaAPI.Utilities.Log;
namespace GameServer.Game.Manager
{
    public class PlayerMailBox 
    {
        public const uint UPDATE_TIME_INTERVALE_HOUR = 24u;
        private DateTime m_last_update;
        public const ulong EXPIRES_CACHE_TIME = 3 * 1000Ul; // 3 Segundos
        public const uint NUM_OF_EMAIL_PER_PAGE = 20u; // 20 Emails por p�gina
        public const uint LIMIT_OF_UNREAD_EMAIL = 300u; // 300 Emails n�o lidos que pode enviar para o player                                                                                                   
        protected uint m_uid;                                                                           
        protected Dictionary<uint, EmailInfoEx> m_emails = new Dictionary<uint, EmailInfoEx>();    

        public PlayerMailBox()
        {
            this.m_emails = new Dictionary<uint, EmailInfoEx>();
            this.m_uid = 0u;
            this.m_last_update = DateTime.Now;

        }

        public void init(Dictionary<uint, EmailInfoEx> _emails, uint _uid)
        {

            if (m_emails.Count > 0)
            {
                m_emails.Clear();
            }

            m_uid = _uid;
            m_emails = new Dictionary<uint, EmailInfoEx>(_emails);

            // Initialize last update time
            m_last_update = DateTime.Now;
        }
        public void clear()
        {
            m_uid = 0u;

            if (m_emails.Count > 0)
            {
                m_emails.Clear();
            }
        }
        public bool checkLastUpdate()
        {
            return (ulong)(GetLocalTimeDiff(m_last_update).TotalMilliseconds) >= EXPIRES_CACHE_TIME;
        }

        private TimeSpan GetLocalTimeDiff(DateTime lastUpdate)
        {
            return DateTime.UtcNow - lastUpdate; // Dif
        }
        void update()
        {
            if (m_uid == 0u)
                throw new exception("[PlayerMailBox::update][Error] m_uid is invalid(0), call init method to set uid of player.");

            var cmd_mbi2 = new CmdMailBoxInfo2(m_uid);  // Waiter

            NormalManagerDB.add(0, cmd_mbi2, null, null);

            if (cmd_mbi2.getException().getCodeError() != 0)
                throw cmd_mbi2.getException();

            if (!(m_emails.Count == 0))
                m_emails.Clear();

            m_emails = cmd_mbi2.getInfo();

            // Update last time update
            m_last_update = DateTime.Now;                                                                                                             

        }

        void CheckAndUpdate()
        {

            if (checkLastUpdate())
                update();
        }

        public List<MailBox> GetPage(uint page)
        {
            var mails = new List<MailBox>();

            if (page == 0u)
            {
                throw new Exception($"[PlayerMailBox::GetPage][Error] Player[UID={m_uid}] Page({page}) invalid page number.");
            }

            // Verifica se o tempo do cache expirou, se sim, atualiza ele novamente
            CheckAndUpdate();

            // Verifica se a lista de emails está vazia
            if (m_emails.Count == 0)
            {
                return mails; // Retorna lista vazia
            }

            // Verifica se a página existe
            if (page > Math.Ceiling(m_emails.Count / (double)NUM_OF_EMAIL_PER_PAGE))
            {
                throw new Exception($"[PlayerMailBox::GetPage][Error] Player[UID={m_uid}] Page({page}) not exists.");
            }

            // Calcula o índice inicial e a quantidade de emails a pegar
            int startIndex = (int)((page - 1) * NUM_OF_EMAIL_PER_PAGE);
            int count = (int)Math.Min(NUM_OF_EMAIL_PER_PAGE, m_emails.Count - startIndex);

            // Seleciona os emails para a página atual
            var selectedEmails = m_emails
                .Reverse() // Reverte a lista para simular `rbegin` do C++
                .Skip(startIndex)
                .Take(count);

            foreach (var email in selectedEmails)
            {
                var mail = new MailBox { id = 0u }; // Inicializa MailBox
                CopyEmailInfoExToMailBox(email.Value, mail); // Copia dados do email para o MailBox
                mails.Add(mail);
            }

            // Classifica os emails (se necessário)
            mails.Sort((rhs, lhs) => lhs.id.CompareTo(rhs.id));

            return mails;
        }       

        public List<MailBox> getAllUnreadEmail()
        {
            List<MailBox> unread = new List<MailBox>();

            try
            {
                // Verifica se o tempo do cache expirou, se sim, atualiza ele novamente
                CheckAndUpdate();

                if (m_emails.Count == 0)
                    return unread;

                foreach (var email in m_emails.Values.Reverse()) // Itera de trás para frente
                {
                    if (email.lida_yn == 1)
                        continue; // Já foi lido

                    MailBox mail = new MailBox();
                    CopyEmailInfoExToMailBox(email, mail);
                    unread.Add(mail);

                    // Verifica se chegou no limite de emails não lidos
                    if (unread.Count >= LIMIT_OF_UNREAD_EMAIL)
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] GetAllUnreadEmail: {ex.Message}");
            }

            // Classifica pelo último email a chegar na caixa de entrada
            unread.Sort((lhs, rhs) => rhs.id.CompareTo(lhs.id));

            return unread;
        }

        // Método para calcular o número total de páginas
        public uint getTotalPages()
        {
            uint totalPages = 0;

            try
            {
                // Verifica se o tempo do cache expirou, se sim, atualiza ele novamente
                CheckAndUpdate();

                totalPages = (uint)Math.Ceiling(m_emails.Count / (float)NUM_OF_EMAIL_PER_PAGE);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] GetTotalPages: {ex.Message}");
            }

            return totalPages;
        }

        // Método para adicionar um novo email que chegou
        public void addNewEmailArrived(uint emailId)
        {
            try
            {
                if (emailId <= 0)
                    throw new Exception($"[PlayerMailBox::AddNewEmailArrived][Error] Player[UID={m_uid}] email id({emailId}) is invalid.");

                // Verifica se o email já existe no cache
                if (m_emails.ContainsKey(emailId))
                    return; // O cache já foi atualizado

                // Verifica se o tempo do cache expirou, se sim, atualiza ele novamente
                CheckAndUpdate();

                // Verifica se o email ainda não está no cache
                if (!m_emails.ContainsKey(emailId))
                {
                    // Cria o comando para obter informações sobre o email
                    var cmdEmailInfo2 = new CmdEmailInfo2(m_uid, emailId); // Waiter

                    // Simula a interação com o banco de dados
                    NormalManagerDB.add(0, cmdEmailInfo2, null, null);
                                      
                    if (cmdEmailInfo2.getException().getCodeError() != 0)
                        throw cmdEmailInfo2.getException();

                    // Adiciona o novo email ao cache
                    m_emails.Add(cmdEmailInfo2.getInfo().id, cmdEmailInfo2.getInfo());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PlayerMailBox::AddNewEmailArrived][Error] {ex.Message}");
                // Relança a exceção caso seja necessário
                throw;
            }
        }


        // Método para obter informações sobre um email específico
        public EmailInfo getEmailInfo(uint _id, bool _ler)
        {
            var emailInfo = new EmailInfo();

            try
            {
                if (_id <= 0)
                    throw new Exception($"[PlayerMailBox::GetEmailInfo][Error] Player[UID={m_uid}] email id({_id}) is invalid.");

                // Verifica se o tempo do cache expirou, se sim, atualiza ele novamente
                CheckAndUpdate();

                // Verifica se a caixa de entrada está vazia
                if (m_emails.Count == 0)
                    throw new Exception($"[PlayerMailBox::GetEmailInfo][Error] Player[UID={m_uid}] mail box empty, not have how find email id({_id}).");

                if (m_emails.TryGetValue(_id, out var email))
                {
                    if (_ler)
                    {
                        // Marca como lido
                        if (email.lida_yn== 0)
                        {
                            email.lida_yn = 1;
                        }

                        email.visit_count++;

                        // Atualiza no banco de dados
                        NormalManagerDB.add(3, new CmdUpdateEmail(m_uid, email), SQLDBResponse, this);
                    }

                    // Copia os dados
                    emailInfo = email;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PlayerMailBox::GetEmailInfo][Error] {ex.Message}");
                throw;
            }

            return emailInfo;
        }

        public void leftItensFromEmail(uint emailId)
        {
            try
            {
                if (emailId <= 0)
                    throw new Exception($"[PlayerMailBox::LeftItensFromEmail][Error] Player[UID={m_uid}] email id({emailId}) is invalid.");

                // Verifica se o tempo do cache expirou, se sim, atualiza ele novamente
                CheckAndUpdate();

                // Verifica se a caixa de entrada está vazia
                if (m_emails.Count == 0)
                    throw new Exception($"[PlayerMailBox::LeftItensFromEmail][Error] Player[UID={m_uid}] mail box empty, not have how delete items from email id({emailId}).");

                if (m_emails.TryGetValue(emailId, out var email) && email.itens.Count > 0)
                {
                    // Limpa os itens
                    email.itens.Clear();
                }

                // Atualiza no banco de dados
                NormalManagerDB.add(1, new CmdItemLeftFromEmail(emailId), SQLDBResponse, this);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PlayerMailBox::LeftItensFromEmail][Error] {ex.Message}");
                throw;
            }
        }

        public void deleteEmail(uint[] emailIds, uint count)
        {
            try
            {
                if (emailIds == null)
                    throw new Exception($"[PlayerMailBox::DeleteEmail][Error] Player[UID={m_uid}] email[ptr(null), count({count})] is invalid.");

                // Verifica os IDs dos e-mails
                foreach (var id in emailIds)
                {
                    if (id <= 0)
                        throw new Exception($"[PlayerMailBox::DeleteEmail][Error] Player[UID={m_uid}] email[id({id}), count({count})] is invalid.");
                }

                // Verifica se o tempo do cache expirou, se sim, atualiza ele novamente
                CheckAndUpdate();

                // Verifica se a caixa de entrada está vazia
                if (m_emails.Count == 0)
                    throw new Exception($"[PlayerMailBox::DeleteEmail][Error] Player[UID={m_uid}] mail box empty, not have how delete email id(s){{{string.Join(", ", emailIds)}}}.");

                foreach (var emailId in emailIds)
                {
                    if (!m_emails.Remove(emailId))
                    {
                        // Log: email não encontrado
                        _smp.message_pool.push(new message($"[PlayerMailBox::DeleteEmail][Error][WARNING] Player[UID={m_uid}] não encontrou o Email[id({emailId})].", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        continue;
                    }
                }

                // Atualiza no banco de dados
                NormalManagerDB.add(2, new CmdDeleteEmail(m_uid, emailIds, count), SQLDBResponse, this);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PlayerMailBox::DeleteEmail][Error] {ex.Message}");
                throw;
            }
        }

        private void CopyEmailInfoExToMailBox(EmailInfoEx email, MailBox mail)
        {
            // Copia os dados básicos
            mail.id = email.id;
            mail.from_id = (string)email.from_id.Clone(); // Assumindo que é uma string
            mail.msg = (string)email.msg.Clone(); // Assumindo que é uma string
            mail.visit_count = email.visit_count;
            mail.lida_yn = email.lida_yn;

            // Copia os itens
            mail.item_num = (uint)email.itens.Count;

            if (email.itens.Count > 0)
            {
                mail.item = email.itens.First();
            }
        }                                  

        public void SQLDBResponse(int _msg_id, Pangya_DB _pangya_db, object _arg)
        {
            try
            {
                if (_arg == null)
                {
                    _smp.message_pool.push(new message($"[PlayerMailBox::SQLDBResponse][WARNING] arg is null for msg_id = {_msg_id}", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    return;
                }

                var pmb = (PlayerMailBox)_arg;

                // Verifica se houve erro no banco de dados
                if (_pangya_db.getException().getCodeError() != 0)
                {
                    _smp.message_pool.push(new message($"[PlayerMailBox::SQLDBResponse][Error] Player[UID={pmb.m_uid}] {_pangya_db.getException().getFullMessageError()}", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    return;
                }

                switch (_msg_id)
                {
                    case 1:
                        {
                            var cmdIlfe = (Cmd.CmdItemLeftFromEmail)_pangya_db;

                            _smp.message_pool.push(new message($"[PlayerMailBox::SQLDBResponse][Log] Player[UID={pmb.m_uid}] successfully retrieved items from Email[ID={cmdIlfe.getEmailID()}].", type_msg.CL_FILE_LOG_AND_CONSOLE));
                            break;
                        }
                    case 2:
                        {
                            var cmdDe = (CmdDeleteEmail)_pangya_db;

                            // Lambda para formatar os IDs dos e-mails
                            Func<uint[], uint, string> lambdaPrintAllEmailIds = (emailIds, count) =>
                            {
                                var ret = "";
                                if (emailIds == null) return ret;

                                for (var i = 0; i < count; i++)
                                    ret += (i == 0) ? emailIds[i].ToString() : $", {emailIds[i]}";

                                return ret;
                            };

                            _smp.message_pool.push(new message($"[PlayerMailBox::SQLDBResponse][Log] Player[UID={pmb.m_uid}] successfully deleted email ID(s){{{lambdaPrintAllEmailIds(cmdDe.getEmailID(), cmdDe.getCount())}}}.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                            break;
                        }
                    case 3:
                        {
                            var cmdUe = (Cmd.CmdUpdateEmail)_pangya_db;

                            _smp.message_pool.push(new message($"[PlayerMailBox::SQLDBResponse][Log] Player[UID={cmdUe.getUID()}] successfully updated Email[ID={cmdUe.getEmail().id}, LIDA_YN={(ushort)cmdUe.getEmail().lida_yn}, VISIT_COUNT={cmdUe.getEmail().visit_count}].", type_msg.CL_FILE_LOG_AND_CONSOLE));
                            break;
                        }
                    case 0:
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                _smp.message_pool.push(new message($"[PlayerMailBox::SQLDBResponse][Error] QUERY_MSG[ID={_msg_id}] {e.Message}", type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }            
    }
}