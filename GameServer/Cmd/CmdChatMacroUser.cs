using PangyaAPI.SQL;

namespace GameServer.Cmd
{
    public class CmdChatMacroUser : PangyaAPI.Network.Cmd.CmdChatMacroUser
    {
        public CmdChatMacroUser(uint _uid) : base((int)_uid)
        {
            
        }
    }
}