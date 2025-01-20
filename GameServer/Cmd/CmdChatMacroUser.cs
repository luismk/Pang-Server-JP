using PangyaAPI.SQL;

namespace GameServer.Cmd
{
    public class CmdChatMacroUser : PangyaAPI.TCP.Cmd.CmdChatMacroUser
    {
        public CmdChatMacroUser(uint _uid) : base((int)_uid)
        {
            
        }
    }
}