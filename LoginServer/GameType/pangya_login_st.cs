using LoginServer.PangyaEnums;
using PangyaAPI.Network.Pangya_St;
using PangyaAPI.Utilities.BinaryModels;
using System;
using System.Runtime.InteropServices;

namespace LoginServer.GameType
{                 
    // PlayerInfo
    public class player_info
    {
        public player_info(uint _ul = 0u)
        {
            clear();
        }
        public void clear()
        {
            block_flag = new BlockFlag();
             id = "";
            nickname = "";
            pass = "";
        }

        public void set_info(player_info info)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info), "O parâmetro 'info' não pode ser nulo.");

            uid = info.uid;
            m_cap = info.m_cap;
            block_flag = info.block_flag != null ? info.block_flag : new BlockFlag();
            pass = info.pass; 
            level = info.level; 
            id = info.id;
            nickname = info.nickname;
        }
        public uint uid;
        public uint m_cap;
        public BlockFlag block_flag = new BlockFlag();
      
        public ushort level; 
        public string id = "";
        public string nickname = "";
        public string pass = "";
        public DateTime login_time = DateTime.Now;
        public string acess_code = "302540";///chave de acesso no web cookies, esta fixo ate entao
    }
    public class LoginData
    {
        public string id { get; set; }
        public string password { get; set; }//
        public byte opt_count { get; set; }
        public long[] v_opt_unkn { get; set; } = new long[2];
        public string mac_address { get; set; }
        public new string ToString()
        {
            string data = $": [USER = {id}], [PWD = {password}], [MAC = {mac_address}]";
            return data;
        }
    }
}
