using System;

namespace Pangya_GameServer.PangyaEnums
{
    [Flags]
    public enum CapabilityFlags : uint
    {
        A_I_MODE = 1 << 0,                 // 0x00000001 - Inteligência Artificial Modo
        GALLERY = 1 << 1,                  // 0x00000002 - Unknown
        GAME_MASTER = 1 << 2,              // 0x00000004 - GM(Game Master)
        GM_EDIT_SITE = 1 << 3,             // 0x00000008 - Pode editar a parte adm do site
        BLOCK_GIVE_ITEM_GM = 1 << 4,       // 0x00000010 - Bloqueia o GM de enviar itens
        UNKNOWN_1 = 1 << 5,                // 0x00000020 - Unknown
        MOD_SYSTEM_EVENT = 1 << 6,         // 0x00000040 - Moderador de sistema
        GM_NORMAL = 1 << 7,                // 0x00000080 - GM player normal
        BLOCK_GIFT_SHOP = 1 << 8,          // 0x00000100 - Bloqueia envio de presente no shop
        LOGIN_TEST_SERVER = 1 << 9,        // 0x00000200 - Login em servidores de teste
        MANTLE = 1 << 10,                  // 0x00000400 - Entra em servidores escondidos
        UNKNOWN_3 = 1 << 11,               // 0x00000800 - Unknown
        UNKNOWN_4 = 1 << 12,               // 0x00001000 - Unknown (Faltando no original)
        UNKNOWN_5 = 1 << 13,               // 0x00002000 - Unknown (Faltando no original)
        PREMIUM_USER = 1 << 14,            // 0x00004000 - Usuário premium
        TITLE_GM = 1 << 15                 // 0x00008000 - Título de GM
    }

}
