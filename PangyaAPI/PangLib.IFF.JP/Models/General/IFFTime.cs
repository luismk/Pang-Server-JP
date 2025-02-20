using System;
using System.Runtime.InteropServices;
namespace PangLib.IFF.JP.Models.General
{
    /// <summary>
    /// System time structure based on Windows internal SYSTEMTIME struct
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 16)]
    public class IFFTime
    {
        /// <summary>
        /// Year
        /// </summary>
        public ushort Year { get; set; }

        /// <summary>
        /// Month
        /// </summary>
        public ushort Month { get; set; }

        /// <summary>
        /// Day of Week
        /// </summary>
        public ushort DayOfWeek { get; set; }

        /// <summary>
        /// Day
        /// </summary>
        public ushort Day { get; set; }

        /// <summary>
        /// Hour
        /// </summary>
        public ushort Hour { get; set; }

        /// <summary>
        /// Minute
        /// </summary>
        public ushort Minute { get; set; }

        /// <summary>
        /// Second
        /// </summary>
        public ushort Second { get; set; }

        /// <summary>
        /// Millisecond
        /// </summary>
        public ushort MilliSecond { get; set; }

        public bool TimeActive
        {
            get
            {
                return Year > 0 && Month > 0 && Day > 0;
            }
        }

        public DateTime Time
        {
            get
            {
                if (TimeActive)//normal item
                {
                    return new DateTime(Year, Month, Day, Hour, Minute, Second, MilliSecond);
                }
                //for grand prix :D
                else if(Hour > 0 || Minute > 0)
                {

                    var value = DateTime.Now; Year = (ushort)value.Year;
                    Month = (ushort)value.Month;
                    DayOfWeek = (ushort)value.DayOfWeek;
                    Day = (ushort)value.Day;
                    return new DateTime(value.Year, value.Month, value.Day, Hour, Minute, 0, 0);//aqui tem que setar, dia mes e ano
                }
                return DateTime.Now;
            }
            set
            {
                Year = (ushort)value.Year;
                Month = (ushort)value.Month;
                DayOfWeek = (ushort)value.DayOfWeek;
                Day = (ushort)value.Day;
                Hour = (ushort)value.Hour;
                Minute = (ushort)value.Minute;
                MilliSecond = (ushort)value.Millisecond == 0? (ushort)DateTime.Now.Millisecond: (ushort)value.Millisecond;
                Second = (ushort)value.Second;
            }
        }
        public DateTime TimeGP
        {
            get
            {
                if (Year == 0 && Month == 0 && Day == 0)
                    return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Hour, Minute, Second, MilliSecond);
                else
                return new DateTime(Year, Month, Day, Hour, Minute, Second, MilliSecond);//aqui tem que setar, dia mes e ano
            }
            set
            {
                Year = 0;        // Ano
                Month = 0;         // Mês
                DayOfWeek = 0;      // Dia da semana (não utilizado aqui)
                Day = 0;           // Dia do mês     
                Hour = (ushort)value.Hour;
                Minute = (ushort)value.Minute;
                Second = (ushort)value.Second;
            }
        }

        internal DateTime CheckAndReset()
        {
            Year = (ushort)DateTime.Now.Year;        // Ano
            Month = (ushort)DateTime.Now.Month;         // Mês
            DayOfWeek = (ushort)DateTime.Now.DayOfWeek;      // Dia da semana (não utilizado aqui)
            Day = (ushort)DateTime.Now.Day;           // Dia do mês
            
            // Criação de um novo DateTime com os valores decodificados
            var value = new DateTime(Year, Month, Day, Hour, Minute, Second, MilliSecond);

            // Retorna o novo DateTime
            return value;

        }

        public void ClearTime()
        {
            Year = 0;
            Month = 0;
            DayOfWeek = 0;
            Day = 0;
            Hour = 0;
            Minute = 0;
            Second = 0;
        }

        public string ToString(string format)
        {
            return Time.ToString(format);
        }
        public string GPToString()
        {
            return TimeGP.ToString();
        }
        public DateTime ConvertTime()
        {
            return new DateTime(Year, Month, Day, Hour, Minute, Second, MilliSecond);
        }

        public void CreateTime(string format)
        {
            var date = DateTime.Parse(format);

            Year = (ushort)date.Year;
            Month = (ushort)date.Month;
            Minute = (ushort)date.Minute;
            Day = (ushort)date.Day;
            Hour = (ushort)date.Hour;
            Second = (ushort)date.Second;
            MilliSecond = (ushort)date.Millisecond;
        }

        public void CreateTime()
        {
            var date = DateTime.Now;

            Year = (ushort)date.Year;
            Month = (ushort)date.Month;
            Minute = (ushort)date.Minute;
            Day = (ushort)date.Day;
            Hour = (ushort)date.Hour;
            Second = (ushort)date.Second;
            MilliSecond = (ushort)date.Millisecond;
        }

        public void CreateTime(DateTime date)
        {
            if (date != DateTime.MinValue)
            {

                Year = (ushort)date.Year;
                Month = (ushort)date.Month;
                Minute = (ushort)date.Minute;
                Day = (ushort)date.Day;
                Hour = (ushort)date.Hour;
                Second = (ushort)date.Second;
                MilliSecond = (ushort)date.Millisecond;

            }
        }

        public IFFTime()
        { }
        public IFFTime
            (DateTime date)
        {
            Time = date;
        }

    }
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 36)]
    public class IFFDate
    {
        public IFFDate()
        {
            Start = new IFFTime();
            End = new IFFTime();
        }
        //-------------------- TIME IFF--------------\\
        [field: MarshalAs(UnmanagedType.Bool, SizeConst = 4)]
        public bool active { get; set; }//156 start position
        [field: MarshalAs(UnmanagedType.Struct, SizeConst = 16)]
        public IFFTime Start { get; set; }// 160 start position
        [field: MarshalAs(UnmanagedType.Struct, SizeConst = 16)]
        public IFFTime End { get; set; }// 176 start position
        //--------------------------------------------------\\
        public bool Check()
        {
            if (active)
            {
                return true;
            } 
            return false;
        }
        public void Clear()
        {
            Start = new IFFTime();
            End = new IFFTime();
        }
    }
}
