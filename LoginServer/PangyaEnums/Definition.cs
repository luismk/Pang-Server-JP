using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginServer.PangyaEnums
{
    public enum PacketIDClient
    {
        /// <summary>
        /// Player digita o usuário e senha e clica em login
        /// </summary>
        PLAYER_LOGIN = 0x01,

        /// <summary>
        /// Player Seleciona um Servidor para entrar
        /// </summary>
        PLAYER_SELECT_SERVER = 0x03,

        /// <summary>
        /// login com duplicidade 
        /// </summary>
        PLAYER_DUPLICATE_LOGIN = 0x04,

        /// <summary>
        /// Seta primeiro nickname do usuário
        /// </summary>
        PLAYER_SET_NICKNAME = 0x06,

        /// <summary>
        /// Ocorre quando o cliente clica em Confirmar (se o nickname está disponível), 
        /// </summary>
        PLAYER_CONFIRM_NICKNAME = 0x07,

        /// <summary>
        /// Player selecionou seu primeiro personagem
        /// </summary>
        PLAYER_SELECT_CHARACTER = 0x08,

        /// <summary>
        /// envia chave de autenficação do login e lista novamente os servers
        /// </summary>
        PLAYER_RECONNECT = 0x0B,

        /// <summary>
        /// ?????????
        /// </summary>
        NOTHING = 0xFF
    }

    public enum NICK_CHECK : byte
    {
        SUCCESS,                // Sucesso por trocar o nick por que ele está disponível
        UNKNOWN_ERROR,          // Erro desconhecido, Error ao verificar NICK
        NICK_IN_USE,            // NICKNAME já em uso
        INCORRECT_NICK,         // INCORRET nick, tamanho < 4 ou tem caracteres que não pode
        NOT_ENOUGH_COOKIE,      // Não tem points suficiente
        HAVE_BAD_WORD,          // Tem palavras que não pode no NICK
        ERROR_DB,               // Erro DB
        EMPETY_ERROR,           // Erro Vazio
        EMPETY_ERROR_2,         // ERRO VAZIO 2
        SAME_NICK_USED,         // O Mesmo nick vai ser usado, estou usando para o mesmo que o ID
        EMPETY_ERROR_3,         // ERRO VAZIO 3
        CODE_ERROR_INFO = 12    // CODE  ERROR INFO arquivo iff, o código do erro para mostra no cliente
    }

    /// <summary>
    /// Define o tipo de mensagem a ser exibida para o usuário no momento do Login
    /// 0x01, 0x00, EnumValue, 0x00, 0x00, 0x00, 0x00 
    /// </summary>
    public enum SubLoginCode
    {
        InvalidoIdPw = 0x01,
        InvalidoId = 0x02,
        UsuarioEmUso = 0x04,
        Banido = 0x05,
        UsernameOuSenhaInvalido = 0x06,
        ContaSuspensa = 0x07,
        Player13AnosOuMenos = 0x09,
        SSNIncorreto = 0x0C,
        UsuarioIncorreto = 0x0D,
        OnlyUserAllowed = 0x0E,
        ServerInMaintenance = 0x0F, //Cannot login due to server maintenance
        NaoDisponivelNaSuaArea = 0x10, //By LuisMk
        CreateNickName_US = 0xD8, //by LuisMK (usado no US)
        CreateNickName = 0xD9, //by LuisMK (usado no TH)
    }
}
