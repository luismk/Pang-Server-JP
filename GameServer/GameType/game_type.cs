using GameServer.Game.Utils;
using GameServer.Session;
using PangyaAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GameServer.GameType.ShotSyncData;

namespace GameServer.GameType
{

    public class uSpecialShot
    {
        public uSpecialShot()
        {
            clear();
        }
        void clear()
        {
            ulSpecialShot = 0;
        }
        uint ulSpecialShot;
        public uint spin_front { get => (ulSpecialShot & (1 << 0)) != 0 ? 1U : 0U; set { if (value != 0) ulSpecialShot |= (1 << 0); else ulSpecialShot &= ~(1U << 0); } }
        public uint spin_back { get => (ulSpecialShot & (1 << 1)) != 0 ? 1U : 0U; set { if (value != 0) ulSpecialShot |= (1 << 1); else ulSpecialShot &= ~(1U << 1); } }
        public uint curve_left { get => (ulSpecialShot & (1 << 2)) != 0 ? 1U : 0U; set { if (value != 0) ulSpecialShot |= (1 << 2); else ulSpecialShot &= ~(1U << 2); } }
        public uint curve_right { get => (ulSpecialShot & (1 << 3)) != 0 ? 1U : 0U; set { if (value != 0) ulSpecialShot |= (1 << 3); else ulSpecialShot &= ~(1U << 3); } }
        public uint tomahawk { get => (ulSpecialShot & (1 << 4)) != 0 ? 1U : 0U; set { if (value != 0) ulSpecialShot |= (1 << 4); else ulSpecialShot &= ~(1U << 4); } }
        public uint cobra { get => (ulSpecialShot & (1 << 5)) != 0 ? 1U : 0U; set { if (value != 0) ulSpecialShot |= (1 << 5); else ulSpecialShot &= ~(1U << 5); } }
        public uint spike { get => (ulSpecialShot & (1 << 6)) != 0 ? 1U : 0U; set { if (value != 0) ulSpecialShot |= (1 << 6); else ulSpecialShot &= ~(1U << 6); } }
        public uint _unused = 25; // Não usa 
        public string toString()
        {
            return "Spin Front: " + (spin_front) + " Spin Back: " + (spin_back) + " Curve Left: " + (curve_left) + " Curve Right: " + (curve_right) + " Tomahwak: " + (tomahawk) + " Cobra: " + (cobra) + " Spike: " + (spike) + " Unused: " + (_unused);
        }
    }

    public class ShotDataBase
    {
        public ShotDataBase(uint _ul = 0)
        {
            clear();
        }
        public void clear()
        {
        }
        public string toString()
        {
            return "Bar Point: Forca: " + Convert.ToString(bar_point[0]) + " Hit PangYa: " + Convert.ToString(bar_point[1]) + Environment.NewLine + "Ball Effect: X: " + Convert.ToString(ball_effect[0]) + " Y: " + Convert.ToString(ball_effect[1]) + Environment.NewLine + "Acerto PangYa Flag: " + Convert.ToString((ushort)acerto_pangya_flag) + Environment.NewLine + "Special Shot: " + special_shot.toString() + Environment.NewLine + "Time Hole SYNC: " + Convert.ToString(time_hole_sync) + Environment.NewLine + "Mira(shot): " + Convert.ToString(mira) + Environment.NewLine + "Time Shot: " + Convert.ToString(time_shot) + Environment.NewLine + "Bar Point: Start: " + Convert.ToString(bar_point1) + Environment.NewLine + "Club: " + Convert.ToString((ushort)club) + Environment.NewLine + "fUnknown: [1]: " + Convert.ToString(fUnknown[0]) + " [2]: " + Convert.ToString(fUnknown[1]) + Environment.NewLine + "Impact Zone Size Pixel: " + Convert.ToString(impact_zone_pixel) + Environment.NewLine + "Natural Wind: X: " + Convert.ToString(natural_wind[0]) + " Y: " + Convert.ToString(natural_wind[1]) + Environment.NewLine;
        }
        public float[] bar_point = new float[2]; // [0] 2 Força, [1] 3 Impact Zone
        public float[] ball_effect = new float[2]; // [0] X Spin,  [1] Y Spin
        public byte acerto_pangya_flag;
        public uSpecialShot special_shot = new uSpecialShot(); // Especial Short, Tomahawk, Cobra e Spike
        public uint time_hole_sync = new uint();
        public float mira; // Mira da tacada do player, seria o R do location[x,y,z,r]
        public uint time_shot = new uint();
        public float bar_point1;
        public byte club;
        public float[] fUnknown = new float[2]; // Float Unknown [0] 1 unknown, [1] 2 unknown
        public float impact_zone_pixel;
        public int[] natural_wind = new int[2]; // Natural Wind Valor [0] X valor, [1] Y valor
    }

    // Separei o spand time, que o pang battle não tem ele
    public class ShotData : ShotDataBase
    {
        public ShotData(uint _ul = 0) : base(_ul)
        {
            clear();
        }
        public new string toString()
        {
            return base.toString() + "Spend Time Game: " + Convert.ToString(spend_time_game) + Environment.NewLine;
        }
        public float spend_time_game; // O Acumolo de tempo gasto no jogo, é o tempo decorrido geral
    }

    public class ShotDataEx : ShotData
    {
        public ShotDataEx(uint _ul = 0) : base(_ul)
        {
            clear();
        }
        public class PowerShot
        {
            public PowerShot(uint _ul = 0)
            {
                clear();
            }
            public string toString()
            {
                return "Option: " + Convert.ToString((ushort)option) + Environment.NewLine + "Decrease Power Shot: " + Convert.ToString(decrease_power_shot) + Environment.NewLine + "Increase Power Shot: " + Convert.ToString(increase_power_shot) + Environment.NewLine;
            }
            public void clear()
            { }
            public byte option;
            public int decrease_power_shot = new int();
            public int increase_power_shot = new int();
        }
        public new string toString()
        {
            return (option != 0) ? (power_shot.toString() + base.toString()) : base.toString();
        }
        public ushort option;
        public PowerShot power_shot = new PowerShot();
    }

    public class ShotSyncData
    {
        public void clear()
        {
        }
        public class Location
        {
            public void clear()
            { }
            public string toString()
            {
                return "X: " + Convert.ToString(x) + " Y: " + Convert.ToString(y) + " Z: " + Convert.ToString(z);
            }
            public float x;
            public float y;
            public float z;
        }
        public enum SHOT_STATE : byte
        {
            PLAYABLE_AREA = 2,
            OUT_OF_BOUNDS,
            INTO_HOLE,
            UNPLAYABLE_AREA
        }
        public string toString()
        {
            return "OID: " + Convert.ToString(oid) + Environment.NewLine + "Location: " + location.toString() + Environment.NewLine + "STATE: " + Convert.ToString((ushort)state) + Environment.NewLine + "Bunker Flag: " + Convert.ToString((ushort)bunker_flag) + Environment.NewLine + "ucUnknown: " + Convert.ToString((ushort)ucUnknown) + Environment.NewLine + "Pang: " + Convert.ToString(pang) + Environment.NewLine + "Pang Bonus: " + Convert.ToString(bonus_pang) + Environment.NewLine + "State Shot: " + state_shot.toString() + Environment.NewLine + "Tempo Shot: " + Convert.ToString(tempo_shot) + Environment.NewLine + "Grand Prix Penalidade: " + Convert.ToString((ushort)grand_prix_penalidade) + Environment.NewLine;
        }
        public uint oid = new uint();
        public Location location = new Location();
        public SHOT_STATE state = new SHOT_STATE();
        public byte bunker_flag;
        public byte ucUnknown; // Deve ser relacionando ao bunker esses negocios
        public uint pang = new uint();
        public uint bonus_pang = new uint();
        public bool isMakeHole()
        {
            return state_shot.display.acerto_hole == 1u;
        }
        public class stStateShot
        {
            public void clear()
            {
            }
            public class uDisplayState
            {
                void clear()
                {
                    ulState = 0;
                }
                public uint ulState;
                public uint over_drive
                {
                    get { return (byte)((ulState >> 0) & 1); }
                    set { ulState = (ulState & ~(1U << 0)) | ((uint)value << 0); }
                }

                public uint _bit2_unknown
                {
                    get { return (byte)((ulState >> 2) & 1); }
                    set { ulState = (ulState & ~(1U << 2)) | ((uint)value << 2); }
                }

                public uint super_pangya
                {
                    get { return (byte)((ulState >> 3) & 1); }
                    set { ulState = (ulState & ~(1U << 3)) | ((uint)value << 3); }
                }

                public uint special_shot
                {
                    get { return (byte)((ulState >> 4) & 1); }
                    set { ulState = (ulState & ~(1U << 4)) | ((uint)value << 4); }
                }

                public uint beam_impact
                {
                    get { return (byte)((ulState >> 5) & 1); }
                    set { ulState = (ulState & ~(1U << 5)) | ((uint)value << 5); }
                }

                public uint chip_in_17_a_199
                {
                    get { return (byte)((ulState >> 6) & 1); }
                    set { ulState = (ulState & ~(1U << 6)) | ((uint)value << 6); }
                }

                public uint chip_in_200_plus
                {
                    get { return (byte)((ulState >> 7) & 1); }
                    set { ulState = (ulState & ~(1U << 7)) | ((uint)value << 7); }
                }

                public uint long_putt
                {
                    get { return (byte)((ulState >> 8) & 1); }
                    set { ulState = (ulState & ~(1U << 8)) | ((uint)value << 8); }
                }

                public uint acerto_hole
                {
                    get { return (byte)((ulState >> 9) & 1); }
                    set { ulState = (ulState & ~(1U << 9)) | ((uint)value << 9); }
                }

                public uint approach_shot
                {
                    get { return (byte)((ulState >> 10) & 1); }
                    set { ulState = (ulState & ~(1U << 10)) | ((uint)value << 10); }
                }

                public uint chip_in_with_special_shot
                {
                    get { return (byte)((ulState >> 11) & 1); }
                    set { ulState = (ulState & ~(1U << 11)) | ((uint)value << 11); }
                }

                public uint _bit12_unknown
                {
                    get { return (byte)((ulState >> 12) & 1); }
                    set { ulState = (ulState & ~(1U << 12)) | ((uint)value << 12); }
                }

                public uint happy_bonus
                {
                    get { return (byte)((ulState >> 13) & 1); }
                    set { ulState = (ulState & ~(1U << 13)) | ((uint)value << 13); }
                }

                public uint clear_bonus
                {
                    get { return (byte)((ulState >> 14) & 1); }
                    set { ulState = (ulState & ~(1U << 14)) | ((uint)value << 14); }
                }

                public uint aztec_bonus
                {
                    get { return (byte)((ulState >> 15) & 1); }
                    set { ulState = (ulState & ~(1U << 15)) | ((uint)value << 15); }
                }

                public uint recovery_bonus
                {
                    get { return (byte)((ulState >> 16) & 1); }
                    set { ulState = (ulState & ~(1U << 16)) | ((uint)value << 16); }
                }

                public uint chip_in_without_special_shot
                {
                    get { return (byte)((ulState >> 17) & 1); }
                    set { ulState = (ulState & ~(1U << 17)) | ((uint)value << 17); }
                }

                public uint bound_bonus
                {
                    get { return (byte)((ulState >> 18) & 1); }
                    set { ulState = (ulState & ~(1U << 18)) | ((uint)value << 18); }
                }

                public uint _bit19_unknown
                {
                    get { return (byte)((ulState >> 19) & 1); }
                    set { ulState = (ulState & ~(1U << 19)) | ((uint)value << 19); }
                }

                public uint _bit20_unknown
                {
                    get { return (byte)((ulState >> 20) & 1); }
                    set { ulState = (ulState & ~(1U << 20)) | ((uint)value << 20); }
                }

                public uint mascot_bonus_with_pangya
                {
                    get { return (byte)((ulState >> 21) & 1); }
                    set { ulState = (ulState & ~(1U << 21)) | ((uint)value << 21); }
                }

                public uint mascot_bonus_without_pangya
                {
                    get { return (byte)((ulState >> 22) & 1); }
                    set { ulState = (ulState & ~(1U << 22)) | ((uint)value << 22); }
                }

                public uint special_bonus_with_pangya
                {
                    get { return (byte)((ulState >> 23) & 1); }
                    set { ulState = (ulState & ~(1U << 23)) | ((uint)value << 23); }
                }

                public uint special_bonus_without_pangya
                {
                    get { return (byte)((ulState >> 24) & 1); }
                    set { ulState = (ulState & ~(1U << 24)) | ((uint)value << 24); }
                }

                public uint _bit25_unknown
                {
                    get { return (byte)((ulState >> 25) & 1); }
                    set { ulState = (ulState & ~(1U << 25)) | ((uint)value << 25); }
                }

                public uint _bit26_unknown
                {
                    get { return (byte)((ulState >> 26) & 1); }
                    set { ulState = (ulState & ~(1U << 26)) | ((uint)value << 26); }
                }

                public uint devil_bonus
                {
                    get { return (byte)((ulState >> 27) & 1); }
                    set { ulState = (ulState & ~(1U << 27)) | ((uint)value << 27); }
                }

                public uint _bit28_a_32_unknown
                {
                    get { return (byte)((ulState >> 28) & 0x1F); }
                    set { ulState = (ulState & ~(0x1F << 28)) | ((uint)value << 28); }
                }

                public void Clear() => ulState = 0;
            }

            public class uShotState
            {
                public uint ulState;

                public uint _bit1_unknown { get => (ulState & (1 << 0)) != 0 ? 1U : 0U; set { if (value != 0) ulState |= (1 << 0); else ulState &= ~(1U << 0); } }
                public uint tomahawk { get => (ulState & (1 << 1)) != 0 ? 1U : 0U; set { if (value != 0) ulState |= (1 << 1); else ulState &= ~(1U << 1); } }
                public uint spike { get => (ulState & (1 << 2)) != 0 ? 1U : 0U; set { if (value != 0) ulState |= (1 << 2); else ulState &= ~(1U << 2); } }
                public uint cobra { get => (ulState & (1 << 3)) != 0 ? 1U : 0U; set { if (value != 0) ulState |= (1 << 3); else ulState &= ~(1U << 3); } }
                public uint spin_front { get => (ulState & (1 << 4)) != 0 ? 1U : 0U; set { if (value != 0) ulState |= (1 << 4); else ulState &= ~(1U << 4); } }
                public uint spin_back { get => (ulState & (1 << 5)) != 0 ? 1U : 0U; set { if (value != 0) ulState |= (1 << 5); else ulState &= ~(1U << 5); } }
                public uint curve_left { get => (ulState & (1 << 6)) != 0 ? 1U : 0U; set { if (value != 0) ulState |= (1 << 6); else ulState &= ~(1U << 6); } }
                public uint curve_right { get => (ulState & (1 << 7)) != 0 ? 1U : 0U; set { if (value != 0) ulState |= (1 << 7); else ulState &= ~(1U << 7); } }
                public uint _bit9_unknown { get => (ulState & (1 << 8)) != 0 ? 1U : 0U; set { if (value != 0) ulState |= (1 << 8); else ulState &= ~(1U << 8); } }
                public uint _bit10_unknown { get => (ulState & (1 << 9)) != 0 ? 1U : 0U; set { if (value != 0) ulState |= (1 << 9); else ulState &= ~(1U << 9); } }
                public uint _bit11_unknown { get => (ulState & (1 << 10)) != 0 ? 1U : 0U; set { if (value != 0) ulState |= (1 << 10); else ulState &= ~(1U << 10); } }
                public uint sem_setas { get => (ulState & (1 << 11)) != 0 ? 1U : 0U; set { if (value != 0) ulState |= (1 << 11); else ulState &= ~(1U << 11); } }
                public uint power_shot { get => (ulState & (1 << 12)) != 0 ? 1U : 0U; set { if (value != 0) ulState |= (1 << 12); else ulState &= ~(1U << 12); } }
                public uint double_power_shot { get => (ulState & (1 << 13)) != 0 ? 1U : 0U; set { if (value != 0) ulState |= (1 << 13); else ulState &= ~(1U << 13); } }
                public uint _bit15_unknown { get => (ulState & (1 << 14)) != 0 ? 1U : 0U; set { if (value != 0) ulState |= (1 << 14); else ulState &= ~(1U << 14); } }
                public uint _bit16_unknown { get => (ulState & (1 << 15)) != 0 ? 1U : 0U; set { if (value != 0) ulState |= (1 << 15); else ulState &= ~(1U << 15); } }
                public uint _bit17_unknown { get => (ulState & (1 << 16)) != 0 ? 1U : 0U; set { if (value != 0) ulState |= (1 << 16); else ulState &= ~(1U << 16); } }
                public uint _bit18_unknown { get => (ulState & (1 << 17)) != 0 ? 1U : 0U; set { if (value != 0) ulState |= (1 << 17); else ulState &= ~(1U << 17); } }
                public uint _bit19_unknown { get => (ulState & (1 << 18)) != 0 ? 1U : 0U; set { if (value != 0) ulState |= (1 << 18); else ulState &= ~(1U << 18); } }
                public uint _bit20_unknown { get => (ulState & (1 << 19)) != 0 ? 1U : 0U; set { if (value != 0) ulState |= (1 << 19); else ulState &= ~(1U << 19); } }
                public uint club_wood { get => (ulState & (1 << 20)) != 0 ? 1U : 0U; set { if (value != 0) ulState |= (1 << 20); else ulState &= ~(1U << 20); } }
                public uint club_iron { get => (ulState & (1 << 21)) != 0 ? 1U : 0U; set { if (value != 0) ulState |= (1 << 21); else ulState &= ~(1U << 21); } }
                public uint club_pw_sw { get => (ulState & (1 << 22)) != 0 ? 1U : 0U; set { if (value != 0) ulState |= (1 << 22); else ulState &= ~(1U << 22); } }
                public uint club_putt { get => (ulState & (1 << 23)) != 0 ? 1U : 0U; set { if (value != 0) ulState |= (1 << 23); else ulState &= ~(1U << 23); } }
                public uint _bit25_a_32_unknown { get => (ulState & (255U << 24)) >> 24; set { ulState = (ulState & ~(255U << 24)) | ((value & 255U) << 24); } }

                public void Clear() => ulState = 0;
            }
            public uDisplayState display = new uDisplayState();
            public uShotState shot = new uShotState();
            public string toString()
            {

                string s = "Display State.\n\r";

                s += "OverDrive: " + Convert.ToString((ushort)display.over_drive) + " SuperPangya: " + Convert.ToString((ushort)display.super_pangya);
                s += " SpecialShot: " + Convert.ToString((ushort)display.special_shot) + " BeamImpact: " + Convert.ToString((ushort)display.beam_impact);
                s += " ChipIn17a199: " + Convert.ToString((ushort)display.chip_in_17_a_199) + " ChipIn200+: " + Convert.ToString((ushort)display.chip_in_200_plus);
                s += " LongPutt: " + Convert.ToString((ushort)display.long_putt) + " AcertoHole: " + Convert.ToString((ushort)display.acerto_hole);
                s += " ApproachShot: " + Convert.ToString((ushort)display.approach_shot) + " ChipInWithSpecialShot(BS,FS): " + Convert.ToString((ushort)display.chip_in_with_special_shot);
                s += " HappyBonus: " + Convert.ToString((ushort)display.happy_bonus) + " ClearBonus: " + Convert.ToString((ushort)display.clear_bonus) + " AztecBonus: " + Convert.ToString((ushort)display.aztec_bonus);
                s += " RecoveryBonus: " + Convert.ToString((ushort)display.recovery_bonus) + " ChipInWithoutSpecialShot: " + Convert.ToString((ushort)display.chip_in_without_special_shot);
                s += " BoundBonus: " + Convert.ToString((ushort)display.bound_bonus);
                s += " MascotBonusWithPangya: " + Convert.ToString((ushort)display.mascot_bonus_with_pangya) + " MascotBonusWithoutPangya: " + Convert.ToString((ushort)display.mascot_bonus_without_pangya);
                s += " SpecialBonusWithPangya: " + Convert.ToString((ushort)display.special_bonus_with_pangya);
                s += " SpecialBonusWithouPangya: " + Convert.ToString((ushort)display.special_bonus_without_pangya);
                s += " DevilBonus: " + Convert.ToString((ushort)display.devil_bonus) + Environment.NewLine;

                s += "Shot State.\n\r";

                s += "Tomahawk: " + Convert.ToString((ushort)shot.tomahawk) + " Spike: " + Convert.ToString((ushort)shot.spike);
                s += " Cobra: " + Convert.ToString((ushort)shot.cobra) + " SpinFront: " + Convert.ToString((ushort)shot.spin_front);
                s += " SpinBack: " + Convert.ToString((ushort)shot.spin_back) + " CurveLeft: " + Convert.ToString((ushort)shot.curve_left);
                s += " CurveRight: " + Convert.ToString((ushort)shot.curve_right) + " SemSetas: " + Convert.ToString((ushort)shot.sem_setas);
                s += " PowerShot: " + Convert.ToString((ushort)shot.power_shot) + " DoublePowerShot: " + Convert.ToString((ushort)shot.double_power_shot);
                s += " ClubWood: " + Convert.ToString((ushort)shot.club_wood) + " ClubIron: " + Convert.ToString((ushort)shot.club_iron);
                s += " ClubPWeSW: " + Convert.ToString((ushort)shot.club_pw_sw) + " ClubPutt: " + Convert.ToString((ushort)shot.club_putt);

                return s;
            }
        }
        public stStateShot state_shot = new stStateShot();
        public ushort tempo_shot; // Acho que seja o tempo da tacada
        public byte grand_prix_penalidade; // Flag(valor) de penalidade do Grand Prix quando tem regras com penalidades
    }

    public class ShotEndLocationData
    {
        public void clear()
        {
        }
        public class stLocation
        {
            public void clear()
            {
            }
            public float x;
            public float y;
            public float z;
            public string toString()
            {
                return "X: " + Convert.ToString(x) + " Y: " + Convert.ToString(y) + " Z: " + Convert.ToString(z);
            }
        }
        public class BallPoint
        {
            public void clear()
            {
            }
            public string toString()
            {
                return "X: " + Convert.ToString(x) + " Y: " + Convert.ToString(y);
            }
            public float x;
            public float y;
        }
        public float porcentagem;
        public stLocation ball_velocity = new stLocation();
        public byte option;
        public stLocation location = new stLocation();
        public stLocation wind_influence = new stLocation();
        public BallPoint ball_point = new BallPoint();
        public uSpecialShot special_shot = new uSpecialShot(); // Tipo da tacada
        public float ball_rotation_spin;
        public float ball_rotation_curve; // Esse é a quantidade do efeito final depois de todos os algorithmos do pangya
        public byte ucUnknown;
        public byte taco; // Club
        public float power_factor;
        public float power_club;
        public float rotation_spin_factor;
        public float rotation_curve_factor;
        public float power_factor_shot;
        public uint time_hole_sync = new uint();
        public string toString()
        {
            return "Porcentagem: " + Convert.ToString(porcentagem) + Environment.NewLine + "Option: " + Convert.ToString((ushort)option) + Environment.NewLine + "Ball Velocity (Initial): " + ball_velocity.toString() + Environment.NewLine + "Location (Begin Shot): " + location.toString() + Environment.NewLine + "Wind Influence: " + wind_influence.toString() + Environment.NewLine + "Ball Point: " + ball_point.toString() + Environment.NewLine + "Special Shot(Tipo da tacada): " + special_shot.toString() + Environment.NewLine + "Ball Rotation (Spin): " + Convert.ToString(ball_rotation_spin) + Environment.NewLine + "Ball Rotation (Curva): " + Convert.ToString(ball_rotation_curve) + Environment.NewLine + "ucUnknown: " + Convert.ToString((ushort)ucUnknown) + Environment.NewLine + "Taco: " + Convert.ToString((ushort)taco) + Environment.NewLine + "Power Factor (Full): " + Convert.ToString(power_factor) + Environment.NewLine + "Power Club(Range): " + Convert.ToString(power_club) + Environment.NewLine + "Rotation Spin Factor: " + Convert.ToString(rotation_spin_factor) + Environment.NewLine + "Rotation Curve Factor: " + Convert.ToString(rotation_curve_factor) + Environment.NewLine + "Power Factor (Shot): " + Convert.ToString(power_factor_shot) + Environment.NewLine + "Time Hole SYNC: " + Convert.ToString(time_hole_sync) + Environment.NewLine;
        }
    }

    public class DropItem
    {
        public enum eTYPE : ulong
        {
            NONE,
            NORMAL_QNTD,
            QNTD_MULTIPLE_500,
            COIN_EDGE_GREEN,
            COIN_GROUND,
            CUBE
        }
        public void clear()
        {
        }
        public uint _typeid = new uint();
        public byte course;
        public byte numero_hole;
        public short qntd;
        public eTYPE type = new eTYPE();
    }

    public class DropItemRet
    {
        public DropItemRet(uint _ul = 0)
        {
            clear();
        }
        public void Dispose()
        {
        }
        public void clear()
        {

            if (v_drop.Any())
            {
                v_drop.Clear();
            }
        }
        public List<DropItem> v_drop = new List<DropItem>();
    }

    public class GameData
    {
        public void clear()
        {
        }
        public uint tacada_num = new uint();
        public uint total_tacada_num = new uint();
        public int score = new int();
        public byte giveup = 1;
        public uint bad_condute = new uint(); // Má conduta, 3 give ups o jogo kika o player
        public uint penalidade = new uint(); // Penalidade do Grand Prix Rule
        public ulong pang = new ulong();
        public ulong bonus_pang = new ulong();
        public long pang_pang_battle = new long(); // Pang do Pang Battle que o player ganhou ou perdeu
        public int pang_battle_run_hole = new int(); // Player saiu do pang battle(-1) ou alguém saiu(+1)
        public uint time_out = new uint(); // Count de time outs do player, 3 time outs o jogo kika o player
        public uint exp = new uint(); // Exp que o player, ganhou no jogo
    }

    public class BarSpace
    {
        public BarSpace(uint _ul = 0)
        {
            clear();
        }
        public void clear()
        {
        }
        public bool setStateAndPoint(byte _state, float _point)
        {

            if (_state > 4)
            {
                return false;
            }

            state = (byte)((_state == 4) ? 3 : _state);

            // Tentou atualizar o State, mas os valores eram diferente, 
            // mas teria que ser o mesmo por que ele só está com lag, pedindo para mandar o pacote de initShot
            if (_state == 4 && point[state] != _point)
            {
                return false;
            }

            point[state] = _point;

            return true;
        }
        public bool setState(byte _state)
        {

            if (_state > 3)
            {
                return false;
            }

            state = _state;

            return true;
        }
        public byte getState()
        {
            return state;
        }

        public float[] getPoint()
        {
            return point;
        }
        public string toString()
        {
            return "Point. Start: " + Convert.ToString(point[0]) + " Impact Zone: " + Convert.ToString(point[1]) + " Forca: " + Convert.ToString(point[2]) + " Hit PangYa: " + Convert.ToString(point[3]);
        }
        protected byte state;
        protected float[] point = new float[4]; // 0 ainda não está tacando, 1 início, 2 força, 3 impact zone
    }

    public class UsedItem
    {
        public void Dispose()
        {
        }
        public void clear()
        {
            if (v_passive.Any())
            {
                v_passive.Clear();
            }

            if (v_active.Any())
            {
                v_active.Clear();
            }
        }
        public class Passive
        {
            public void clear()
            {
            }
            public uint _typeid = new uint();
            public uint count = new uint();
        }
        public class Active
        {
            public void Dispose()
            {
            }
            public void clear()
            {

                _typeid = 0;
                count = 0;

                if (v_slot.Any())
                {
                    v_slot.Clear();
                }
            }
            public uint _typeid = new uint();
            public uint count = new uint();
            public List<byte> v_slot = new List<byte>();
        }
        public class Rate
        {
            public void clear()
            {
                // Default value
                pang = 100;
                exp = 100;
                club = 100;
                drop = 100;
            }
            public uint pang = new uint();
            public uint exp = new uint();
            public uint club = new uint();
            public uint drop = new uint();
        }
        public class ClubMastery
        {
            public void clear()
            {
            }
            public uint _typeid = new uint();
            public uint count = new uint();
            public float rate;
        }
        public Dictionary<uint, Passive> v_passive = new Dictionary<uint, Passive>();
        public Dictionary<uint, Active> v_active = new Dictionary<uint, Active>();
        public Rate rate = new Rate();
        public ClubMastery club = new ClubMastery();
    }

    // Effect Item Flag
    public class uEffectFlag
    {
        public uEffectFlag(ulong _ull = 0)
        {
            ullFlag = (_ull);
        }
        public void clear()
        {
            ullFlag = 0;
        }
        public ulong ullFlag;
        public ulong NONE { get => (ullFlag & (1U << 0)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 0); else ullFlag &= ~(1U << 0); } }
        public ulong PIXEL { get => (ullFlag & (1U << 1)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 1); else ullFlag &= ~(1U << 1); } }
        public ulong PIXEL_BY_WIND_NO_ITEM { get => (ullFlag & (1U << 2)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 2); else ullFlag &= ~(1U << 2); } }
        public ulong PIXEL_OVER_WIND_NO_ITEM { get => (ullFlag & (1U << 3)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 3); else ullFlag &= ~(1U << 3); } }
        public ulong PIXEL_BY_WIND { get => (ullFlag & (1U << 4)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 4); else ullFlag &= ~(1U << 4); } }
        public ulong PIXEL_2 { get => (ullFlag & (1U << 5)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 5); else ullFlag &= ~(1U << 5); } }
        public ulong PIXEL_WITH_WEAK_WIND { get => (ullFlag & (1U << 6)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 6); else ullFlag &= ~(1U << 6); } }
        public ulong POWER_GAUGE_TO_START_HOLE { get => (ullFlag & (1U << 7)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 7); else ullFlag &= ~(1U << 7); } }
        public ulong POWER_GAUGE_MORE_ONE { get => (ullFlag & (1U << 8)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 8); else ullFlag &= ~(1U << 8); } }
        public ulong POWER_GAUGE_TO_START_GAME { get => (ullFlag & (1U << 9)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 9); else ullFlag &= ~(1U << 9); } }
        public ulong PAWS_NOT_ACCUMULATE { get => (ullFlag & (1U << 10)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 10); else ullFlag &= ~(1U << 10); } }
        public ulong SWITCH_TWO_EFFECT { get => (ullFlag & (1U << 11)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 11); else ullFlag &= ~(1U << 11); } }
        public ulong EARCUFF_DIRECTION_WIND { get => (ullFlag & (1U << 12)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 12); else ullFlag &= ~(1U << 12); } }
        public ulong COMBINE_ITEM_EFFECT { get => (ullFlag & (1U << 13)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 13); else ullFlag &= ~(1U << 13); } }
        public ulong SAFETY_CLIENT_RANDOM { get => (ullFlag & (1U << 14)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 14); else ullFlag &= ~(1U << 14); } }
        public ulong PIXEL_RANDOM { get => (ullFlag & (1U << 15)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 15); else ullFlag &= ~(1U << 15); } }
        public ulong WIND_1M_RANDOM { get => (ullFlag & (1U << 16)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 16); else ullFlag &= ~(1U << 16); } }
        public ulong PIXEL_BY_WIND_MIDDLE_DOUBLE { get => (ullFlag & (1U << 17)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 17); else ullFlag &= ~(1U << 17); } }
        public ulong GROUND_100_PERCENT_RONDOM { get => (ullFlag & (1U << 18)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 18); else ullFlag &= ~(1U << 18); } }
        public ulong ASSIST_MIRACLE_SIGN { get => (ullFlag & (1U << 19)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 19); else ullFlag &= ~(1U << 19); } }
        public ulong VECTOR_SIGN { get => (ullFlag & (1U << 20)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 20); else ullFlag &= ~(1U << 20); } }
        public ulong ASSIST_TRAJECTORY_SHOT { get => (ullFlag & (1U << 21)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 21); else ullFlag &= ~(1U << 21); } }
        public ulong PAWS_ACCUMULATE { get => (ullFlag & (1U << 22)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 22); else ullFlag &= ~(1U << 22); } }
        public ulong POWER_GAUGE_FREE { get => (ullFlag & (1U << 23)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 23); else ullFlag &= ~(1U << 23); } }
        public ulong SAFETY_RANDOM { get => (ullFlag & (1U << 24)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 24); else ullFlag &= ~(1U << 24); } }
        public ulong ONE_IN_ALL_STATS { get => (ullFlag & (1U << 25)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 25); else ullFlag &= ~(1U << 25); } }
        public ulong POWER_GAUGE_BY_MISS_SHOT { get => (ullFlag & (1U << 26)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 26); else ullFlag &= ~(1U << 26); } }
        public ulong PIXEL_BY_WIND_2 { get => (ullFlag & (1U << 27)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 27); else ullFlag &= ~(1U << 27); } }
        public ulong PIXEL_WITH_RAIN { get => (ullFlag & (1U << 28)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 28); else ullFlag &= ~(1U << 28); } }
        public ulong NO_RAIN_EFFECT { get => (ullFlag & (1U << 29)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 29); else ullFlag &= ~(1U << 29); } }
        public ulong PUTT_MORE_10Y_RANDOM { get => (ullFlag & (1U << 30)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 30); else ullFlag &= ~(1U << 30); } }
        public ulong UNKNOWN_31 { get => (ullFlag & (1U << 31)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 31); else ullFlag &= ~(1U << 31); } }
        public ulong MIRACLE_SIGN_RANDOM { get => (ullFlag & (1U << 32)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 32); else ullFlag &= ~(1U << 32); } }
        public ulong UNKNOWN_33 { get => (ullFlag & (1U << 33)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 33); else ullFlag &= ~(1U << 33); } }
        public ulong DECREASE_1M_OF_WIND { get => (ullFlag & (1U << 34)) != 0 ? 1U : 0U; set { if (value != 0) ullFlag |= (1U << 34); else ullFlag &= ~(1U << 34); } }

        public static uint enumToBitValue<TEnum>(TEnum enumValue) where TEnum : Enum
        {
            // Desloca o valor 1 para a posição indicada pelo valor do enum
            return (uint)(1 << Convert.ToInt32(enumValue));
        }
    }

    // Bit value                                                          

    public class PlayerGameInfo
    {
        public enum eCARD_WIND_FLAG : byte
        {
            NONE, // Nenhum Efeito
            NORMAL, // Normal, diminui 1m do vento, quando é 9m
            RARE, // Rare, diminui 1m de todos os ventos exceto menos 1m
            SUPER_RARE, // Super Rare, diminui 2m do vento, quando é 6m a 9m
            SECRET // Secret, diminui 1m do vento, quando é 2m a 5m e diminui 2m do vento, quando é 6m a 9m
        }
        public enum eFLAG_GAME : byte
        {
            PLAYING, // Ainda esta jogando
            TICKET_REPORT, // Saiu com ticket report, "Terminou o jogo"
            FINISH, // Jogador terminou o jogo
            BOT, // É Bot do Grand Prix
            QUIT, // Saiu do jogo
            END_GAME // Terminou o jogo, antes do jogar acabar
        }
        public enum eTEAM : byte
        {
            T_RED,
            T_BLUE,
            T_NONE
        }
        public PlayerGameInfo(uint _ul = 0)
        {
            clear();
        }
        public virtual void Dispose()
        {
            clear();
        }
        public void clear()
        {

            uid = 0;
            oid = 0;
            level = 0;
            finish_load_hole = 0;
            finish_char_intro = 0;
            init_shot = 0;
            finish_shot = 0;
            finish_shot2 = 0;
            sync_shot_flag = 0;
            sync_shot_flag2 = 0;
            finish_hole = 0;
            finish_hole2 = 0;
            finish_hole3 = 0;
            finish_game = 0;
            finish_item_used = 0;
            premium_flag = 0;
            trofel = 0;
            char_motion_item = 0;
            assist_flag = 0;
            enter_after_started = 0;
            progress_bar = 0;
            tempo = 0;
            power_shot = 0;
            club = 0;
            chat_block = 0;
            degree = 0;
            mascot_typeid = 0;

            init_first_hole = 0;

            tick_sync_shot.clear();
            tick_sync_end_shot.clear();

            card_wind_flag = eCARD_WIND_FLAG.NONE;
            flag = eFLAG_GAME.PLAYING;
            team = eTEAM.T_NONE; // Valor Padrão

            effect_flag_shot.clear();
            item_active_used_shot = 0;
            earcuff_wind_angle_shot = 0.0f;
            boost_item_flag.clear();

            thi.clear();
            bar_space.clear();
            location.clear();
            data.clear();
            shot_data.clear();
            shot_data_for_cube.clear();
            shot_sync.clear();
            ui.clear();
            drop_list.clear();
            used_item.clear();
            progress.clear();
            medal_win = new uMedalWin();

            typeing = -1;
            hole = -1;
        }
        public class stProgress
        {
            public void clear()
            {
                hole = -1;
            }
            public bool isGoodScore()
            {

                for (var i = 0; i < 18; ++i)
                {
                    if (score[i] > 0)
                    {
                        return false;
                    }
                }

                return true;
            }
            public int getBestRecovery()
            {

                int first = 0;
                int last = 0;
                int i = 0;

                for (i = 0; i < 9; ++i)
                {
                    first += score[i];
                }

                for (i = 9; i < 18; ++i)
                {
                    last += score[i];
                }

                return (first * -1) - last; // Pronto agora ele reflete o quanto que o player recuperou-se
            }
            public short hole; // Hole Atual
            public float best_chipin;
            public float best_long_puttin;
            public float best_drive;
            public byte[] finish_hole = new byte[18]; // Flag para verificar se o player terminou o hole
            public byte[] par_hole = new byte[18]; // Par do hole, [18 Holes o máximo de um jogo]
            public uint[] tacada = new uint[18]; // Tacadas do hole, [18 Holes o máximo de um jogo]
            public string score = new string(new char[18]); // Score do hole, [18 Holes o máximo de um jogo]
        }
        public class stTreasureHunterInfo
        {
            public stTreasureHunterInfo(uint _ul = 0)
            {
                clear();
            }
            public void Dispose()
            {
            }
            public void clear()
            {

                all_score = 0;
                par_score = 0;
                birdie_score = 0;
                eagle_score = 0;

                treasure_point = 0;

                if (v_item.Any())
                {
                    v_item.Clear();
                }
            }
            public uint getPoint(uint _tacada, byte _par_hole)
            {
                byte point = all_score;

                if (_tacada == 1) // HIO
                {
                    return point;
                }

                sbyte score = (sbyte)(_tacada - _par_hole);

                switch (score)
                {
                    case 0: // Par
                        point += par_score;
                        break;
                    case -1: // Birdie
                        point += birdie_score;
                        break;
                    case -2: // Eagle
                        point += eagle_score;
                        break;
                }

                return point;
            }
            public static stTreasureHunterInfo operator +(stTreasureHunterInfo lhs, stTreasureHunterInfo rhs)
            {
                if (lhs == null || rhs == null) return lhs;  // Verifica se algum objeto é null, e retorna o lhs

                lhs.all_score += rhs.all_score;
                lhs.par_score += rhs.par_score;
                lhs.birdie_score += rhs.birdie_score;
                lhs.eagle_score += rhs.eagle_score;
                lhs.treasure_point += rhs.treasure_point;  
                return lhs; // Retorna a instância lhs após a soma
            }                 
            public uint treasure_point = new uint(); // Treasure Hunter point do player no game
            public List<TreasureHunterItem> v_item = new List<TreasureHunterItem>(); // Treasure Hunter Item
            public byte all_score;
            public byte par_score;
            public byte birdie_score;
            public byte eagle_score;
        }
        public class TickTimeSync
        {
            public TickTimeSync(uint _ul = 0)
            {
                clear();
            }
            public void clear()
            {
            }
            public byte count;

            public byte active = 1;
            public ulong tick = new ulong();
        }
        public class uBoostItemFlag
        {
            public void clear()
            {
                ucFlag = 0;
            }
            public byte ucFlag;
            public uint pang { get => (ucFlag & (1 << 0)) != 0 ? 1U : 0U; set { if (value != 0) ucFlag |= (1 << 0); else ucFlag &= (byte)~(1 << 0); } }
            public uint pang_nitro { get => (ucFlag & (1 << 1)) != 0 ? 1U : 0U; set { if (value != 0) ucFlag |= (1 << 1); else ucFlag &= (byte)~(1 << 1); } }
            public uint exp { get => (ucFlag & (1 << 2)) != 0 ? 1U : 0U; set { if (value != 0) ucFlag |= (1 << 2); else ucFlag &= (byte)~(1 << 2); } }

        }
        public uint uid = new uint();
        public uint oid = new uint();
        public byte level;
        public byte hole; // Número do Hole que o player está

        public byte init_first_hole = 1; // Flag que guarda quando o player inicializou o primeiro hole do jogo

        public byte finish_load_hole = 1;

        public byte finish_char_intro = 1;

        public byte init_shot = 1;

        public byte finish_shot = 1;

        public byte finish_shot2 = 1;

        public byte finish_hole = 1; // Usa no Grand Prix, flag de sincronização de hole conluído para trocar para o prox

        public byte finish_hole2 = 1; // Usa no Grand Prix, flag de sincronização do tempo do hole do player, para não dá time out depois que ele concluiu o hole

        public byte finish_hole3 = 1; // Usa no Grand Prix, flag de sincronização se o player já enviou o pacote de finalizar o hole antes

        public byte sync_shot_flag = 1;

        public byte sync_shot_flag2 = 1;

        public byte finish_game = 1; // Terminou o jogo

        public byte assist_flag = 1; // 0 não está com assist ligado, 1 está com assist ligado

        public byte char_motion_item = 1; // Está com intro de character Equipado

        public byte premium_flag = 1; // 1 Player é um usuário premium, 0 player normal

        public byte enter_after_started = 1; // Entrou no Jogo depois de ele ter começado

        public byte finish_item_used = 1; // 1 Player já finalizou os itens usados no jogo, não finalizar de novo se ele já estiver finalizado
        public byte trofel; // Trofel que ele ganhou, 1 ouro, 2 prate, 3 bronze
        public ushort progress_bar;
        public uint tempo = new uint();
        public byte power_shot;
        public byte club; // Taco
        public short typeing; // Escrevendo
        public byte chat_block; // Chat Block
        public ushort degree; // Degree(Graus) do player no Hole
        public uint mascot_typeid = new uint(); // Typeid do Mascot equipado
        public uint item_active_used_shot = new uint(); // O item Active usado na tacada
        public float earcuff_wind_angle_shot; // Ângulo que o efeito earcuff ativou na tacada para o player
        public uEffectFlag effect_flag_shot = new uEffectFlag(); // Effect Flag Shot(tacada), Wind 1m, Safety, Patinha e etc
        public eFLAG_GAME flag = new eFLAG_GAME(); // Flag se acabou o camp, ainda esta jogando, quitou, saiu, ou o jogo terminou pro ele
        public uBoostItemFlag boost_item_flag = new uBoostItemFlag(); // Flag que Exibe os icon de quais boost item o player está usando
        public eCARD_WIND_FLAG card_wind_flag = new eCARD_WIND_FLAG(); // Card Wind Flag
        public stTreasureHunterInfo thi = new stTreasureHunterInfo(); // Treasure Hunter Info do player, esse é que aumenta com card
        public eTEAM team = new eTEAM(); // Team(time) que o player está, antes usado no tourney de time, agora só usado no Match
        public TickTimeSync tick_sync_shot = new TickTimeSync(); // Tick de quando o player recebeu o pacote para ele enviar o pacote sync shot
        public TickTimeSync tick_sync_end_shot = new TickTimeSync(); // Tick de quando o player enviou o pacote de termino de tacada (FinishShot)
        public BarSpace bar_space = new BarSpace();
        public Location location = new Location();
        public GameData data = new GameData();
        public ShotDataEx shot_data = new ShotDataEx();
        public ShotEndLocationData shot_data_for_cube = new ShotEndLocationData(); // Dados que vou usar para os locais de spaw do Spinning Cube
        public ShotSyncData shot_sync = new ShotSyncData();
        public UserInfoEx ui = new UserInfoEx();
        public DropItemRet drop_list = new DropItemRet(); // Drop List do player
        public UsedItem used_item = new UsedItem(); // Item usado no jogo
        public stProgress progress = new stProgress(); // Progresso do jogo, tacadas e score
        public PangyaTime time_finish = new PangyaTime(); // Tempo que acabou o game
        public uMedalWin medal_win = new uMedalWin(); // Medal que Ganhou no Jogo
        //public SysAchievement sys_achieve = new SysAchievement(); // System of Achievement of Player
    }

    // Ticket Report Info
    public class TicketReportInfo
    {
        public TicketReportInfo(uint _ul = 0)
        {
            clear();
        }
        public void Dispose()
        {
        }
        public void clear()
        {
            id = -1;
            v_dados.Clear();
        }
        public class stTicketReportDados
        {
            public void clear()
            {
            }
            public uint uid = new uint();
            public int score = new int();
            public uMedalWin medal = new uMedalWin();
            public byte trofel;
            public ulong pang = new ulong();
            public ulong bonus_pang = new ulong();
            public uint exp = new uint();
            public uint mascot_typeid = new uint();
            public uint flag_item_pang = new uint();
            public uint premium = new uint();
            public uint state = new uint();
            public PangyaTime finish_time = new PangyaTime();
        }
        public int id = new int();
        public List<stTicketReportDados> v_dados = new List<stTicketReportDados>();
    }

    // Enter After Start Info
    public class EnterAfterStartInfo
    {
        public void clear()
        {
        }
        public byte[] tacada = new byte[18]; // 18 Holes
        public int[] score = new int[18]; // 18 Holes
        public ulong[] pang = new ulong[18]; // 18 Holes
        public uint request_oid = new uint();
        public uint owner_oid = new uint();
    }


    public class PlayerOrderTurnCtx
    {
        public PlayerOrderTurnCtx(uint _ul = 0)
        {
            clear();
        }
        public PlayerOrderTurnCtx(PlayerGameInfo _pgi, Hole _hole)
        {
            this.pgi = _pgi;
            this.hole = _hole;
        }
        public void clear()
        {
            pgi = null;
            hole = null;
        }
        public PlayerGameInfo pgi;
        public Hole[] hole;
    }

    // Table Rate Voice And Effect On Versus
    public class TableRateVoiceAndEffect
    {
        public enum eTYPE : byte
        {
            NONE,
            W_BIGBONGDARI,
            R_BIGBONGDARI,
            VOICE_CLUB
        }
        public TableRateVoiceAndEffect(uint _ul = 0)
        {
            clear();
        }
        public TableRateVoiceAndEffect(string _name, eTYPE _type)
        {
            this.name = _name;
            this.type = _type;
            randomTable();
        }
        public void Dispose()
        {
        }
        public void clear()
        {
            name = "";
            type = eTYPE.NONE;
        }
        public void randomTable()
        {

            ushort min_value = 0;

            if (type == eTYPE.VOICE_CLUB)
            {
                min_value = 1;
            }
            for (var i = 0; i < 100; ++i)
            {
                table[i] = (byte)(min_value + ((byte)sRandomGen.getInstance().rIbeMt19937_64_chrono() % (4 - min_value)));
            }
        }
        public string name = "";
        public eTYPE type = new eTYPE();
        public byte[] table = new byte[100];
    }

    public class TreasureHunterVersusInfo
    {
        public TreasureHunterVersusInfo(uint _ul = 0)
        {
            clear();
        }                                          
        public void clear()
        {

            all_score = 0;
            par_score = 0;
            birdie_score = 0;
            eagle_score = 0;

            treasure_point = 0;

            if (v_item.Any())
            {
                v_item.Clear();
            }
        }
        public uint getPoint(uint _tacada, byte _par_hole)
        {
            byte point = all_score;

            if (_tacada == 1) // HIO
            {
                return point;
            }

            sbyte score = (sbyte)(_tacada - _par_hole);

            switch (score)
            {
                case 0: // Par
                    point += par_score;
                    break;
                case (sbyte)-1: // Birdie
                    point += birdie_score;
                    break;
                case (sbyte)-2: // Eagle
                    point += eagle_score;
                    break;
            }

            return point;
        }
        
        public class _stTreasureHunterItem
        {
            public _stTreasureHunterItem(uint _ul = 0)
            {
                clear();
            }
            public _stTreasureHunterItem(uint _uid, TreasureHunterItem _thi)
            {
                this.uid = _uid;
                this.thi = _thi;
            }
            public void clear()
            {
                uid = 0;
                thi.clear();
            }
            public uint uid = new uint(); // Player UID
            public TreasureHunterItem thi = new TreasureHunterItem();
        }
        public uint treasure_point = new uint(); // Treasure Hunter point do player no game
        public List<_stTreasureHunterItem> v_item = new List<_stTreasureHunterItem>(); // Treasure Hunter Item
        public byte all_score;
        public byte par_score;
        public byte birdie_score;
        public byte eagle_score;
    }

    // Ret Finish Shot
    public class RetFinishShot
    {
        public RetFinishShot(uint _ul = 0)
        {
            clear();
        }
        public void clear()
        {
            p = new List<Player>();
        }
        public int ret;
        public List<Player> p;
    }

    // Holes rain count
    public class HolesRain
    {
        public HolesRain(uint _ul = 0)
        {
            clear();
        }
        public void clear()
        {
        }
        public byte getCountHolesRainBySeq(uint _seq)
        {

            // Sequência de hole valor errado
            if (_seq < 1 || _seq > 18)
            {
                return 0;
            }

            byte sum = 0;
            for (uint i = 0; i < _seq; i++)
            {
                sum += rain[i];
            }

            return sum;
        }
        public byte getCountHolesRain()
        {
            byte sum = 0;
            for (uint i = 0; i < rain.Length; i++)
            {
                sum += rain[i];
            }

            return sum;
        }
        public void setRain(uint _index, byte _value)
        {

            // Index invalido
            if ((int)_index < 0 || _index >= 18)
            {
                return;
            }

            rain[_index] = _value;
        }
        protected byte[] rain = new byte[18]; // Máximo número de holes de um jogo
    }

    // Consecutivos Holes Rain(Recovery) Tempo Ruim
    public class ConsecutivosHolesRain
    {
        public ConsecutivosHolesRain(uint _ul = 0)
        {
            clear();
        }
        public void clear()
        {
        }
        public bool isValid()
        {
            return (_4_pluss_count.getCountHolesRain() > 0 || _3_count.getCountHolesRain() > 0 || _2_count.getCountHolesRain() > 0);
        }
        public HolesRain _4_pluss_count = new HolesRain();
        public HolesRain _3_count = new HolesRain();
        public HolesRain _2_count = new HolesRain();
    }
}
