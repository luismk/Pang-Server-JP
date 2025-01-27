﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Xml.Linq;                           
namespace PangyaAPI.Utilities.BinaryModels
{
    public class PangyaBinaryWriter : BinaryWriter
    {
        Encoding _Encoding = Encoding.GetEncoding("Shift_JIS");
        public PangyaBinaryWriter(Stream output) { }
        public PangyaBinaryWriter(Stream output, Encoding encoding) : base(output, encoding)
        {
             
        }

        public PangyaBinaryWriter(Stream output, Encoding encoding, bool leaveOpen) : base(output, encoding, leaveOpen)
        {
        }

        public PangyaBinaryWriter() : base(new MemoryStream(), Encoding.GetEncoding("Shift_JIS"))
        {
            //this.OutStream = new MemoryStream();
            //this._Encoding = Encoding.GetEncoding("Shift_JIS"); // Japan!

        }
        public PangyaBinaryWriter(ushort id) : base(new MemoryStream(), Encoding.GetEncoding("Shift_JIS"))
        {
            init_plain(id);
        }
        public uint GetSize
        {
            get { return (uint)BaseStream.Length; }
        }
        public uint Size
        {
            get { return (uint)BaseStream.Length; }
        }
        public byte[] GetBytes => CreateBytes();


        public void Clear()
        {
            this.Flush();
            this.Close();
            this.OutStream = new MemoryStream();
        }

        public void init_plain(ushort value)
        {
            WriteUInt16(value);                                                             
        }

        public void WriteInt16(short value)
        {
            try
            {
                Write(value);
            }
            catch
            {

            }

        }
        public bool WriteStr(string message, int length)
        {

            try
            {
                if (message == null)
                {
                    message = string.Empty;
                }

                var ret = new byte[length];
                _Encoding.GetBytes(message).Take(length).ToArray().CopyTo(ret, 0);

                Write(ret);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool WriteStr(string message)
        {
            try
            {
                WriteStr(message, message.Length);

            }
            catch
            {
                return false;
            }
            return true;

        }

        public bool WritePStr(string data)
        {
            if (data == null) data = "";
            try
            {
                var encoded = _Encoding.GetBytes(data);
                var length = encoded.Length;
                if (length >= ushort.MaxValue)
                {
                    return false;
                }
                Write((short)length);
                Write(encoded);
            }
            catch
            {
                return false;
            }
            return true;
        }


        public bool WriteBytes(byte[] message, int length)
        {
            try
            {
                if (message == null)
                    message = new byte[length];

                var result = new byte[length];

                Buffer.BlockCopy(message, 0, result, 0, length);

                Write(result);
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool Write(byte[] message, int length)
        {
            try
            {
                if (message == null)
                    message = new byte[length];

                var result = new byte[length];

                Buffer.BlockCopy(message, 0, result, 0, message.Length);

                Write(result);
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool WriteZero(int Lenght)
        {
            try
            {
                Write(new byte[Lenght]);
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool WriteUInt16(ushort value)
        {
            try
            {
                Write(value);
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool WriteUInt16(int value)
        {
            try
            {
                Write((ushort)value);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool WriteUInt16(uint value)
        {
            try
            {
                Write((ushort)value);
            }
            catch
            {
                return false;
            }
            return true;
        }


        public bool WriteByte(byte value)
        {
            try
            {
                Write(value);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool WriteByte(int value)
        {
            try
            {
                Write(Convert.ToByte(value));
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool WriteSingle(float value)
        {
            try
            {
                Write(value);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool WriteUInt32(uint value)
        {
            try
            {
                Write(value);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool WriteInt32(int value)
        {
            try
            {
                Write(value);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool WriteUInt64(ulong value)
        {
            try
            {
                Write(value);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool WriteInt64(long value)
        {
            try
            {
                Write(value);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool WriteDouble(double value)
        {
            try
            {
                Write(value);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool WriteStruct<T>(T[] values)
        {
            // Calcular o tamanho da estrutura
            int size = Marshal.SizeOf<T>();

            // Converter a lista de valores em um array
            var valueList = new List<T>(values);
            int count = valueList.Count;

            // Criar um buffer único para todos os dados
            byte[] buffer = new byte[size * count];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                for (int i = 0; i < count; i++)
                {
                    // Serializar cada estrutura para o ponteiro
                    Marshal.StructureToPtr(valueList[i], ptr, true);
                    // Copiar os dados do ponteiro para o buffer
                    Marshal.Copy(ptr, buffer, i * size, size);
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            Write(buffer);
            // Escrever o buffer
            return true;
        }
       
        public void WriteFile(string file)
        {
            File.WriteAllBytes(file, GetBytes);
        }

        public bool WriteStruct(object value, object value_ori)
        {
            try
            {
                int size = Marshal.SizeOf(value_ori);
                byte[] arr = new byte[size];

                IntPtr ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(value, ptr, false);
                Marshal.Copy(ptr, arr, 0, size);
                Marshal.FreeHGlobal(ptr);                   
                Write(arr, size);
            }
            catch (exception ex)
            {
                var log = "";
                log += ($"message: {ex.Message}");
                log += ($"Source: {ex.Source}");
                log += ($"Method: {ex.TargetSite}");
                log += ($"StackTrace: {ex.StackTrace}");
                Console.WriteLine(log);
                return false;
            }
            return true;
        }                
        public bool WriteBuffer(object value, int size)
        {
            try
            {
                 byte[] arr = new byte[size];

                IntPtr ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(value, ptr, true);
                Marshal.Copy(ptr, arr, 0, size);
                Marshal.FreeHGlobal(ptr);               
                Write(arr);
            }
            catch (exception e)
            {
                throw e;
            }
            return true;
        }
        public bool WriteHexArray(string _value)
        {
            try
            {
                _value = _value.Replace(" ", "");
                int _size = _value.Length / 2;
                byte[] _result = new byte[_size];
                for (int ii = 0; ii < _size; ii++)
                    WriteByte(Convert.ToByte(_value.Substring(ii * 2, 2), 16));
            }
            catch
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Write Pangya Time
        /// </summary>
        /// <returns></returns>
        public bool WriteTime(DateTime? date)
        {
            try
            {
                if (date.HasValue == false || date?.Ticks == 0)
                {
                    Write(new byte[16]);
                    return true;
                }
                WriteUInt16((ushort)date?.Year);
                WriteUInt16((ushort)date?.Month);
                WriteUInt16(Convert.ToUInt16(date?.DayOfWeek));
                WriteUInt16((ushort)date?.Day);
                WriteUInt16((ushort)date?.Hour);
                WriteUInt16((ushort)date?.Minute);
                WriteUInt16((ushort)date?.Second);
                WriteUInt16((ushort)date?.Millisecond);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Write Pangya Time
        /// </summary>
        /// <returns></returns>
        public bool WriteTime()
        {
            DateTime date = DateTime.Now;
            try
            {
                WriteUInt16((ushort)date.Year);
                WriteUInt16((ushort)date.Month);
                WriteUInt16((ushort)date.DayOfWeek);
                WriteUInt16((ushort)date.Day);
                WriteUInt16((ushort)date.Hour);
                WriteUInt16((ushort)date.Minute);
                WriteUInt16((ushort)date.Second);
                WriteUInt16((ushort)date.Millisecond);
                return true;
            }
            catch
            {
                return false;
            }
        }
        byte[] CreateBytes()
        {
            if (OutStream is MemoryStream stream)
                return stream.ToArray();


            using (var memoryStream = new MemoryStream())
            {
                memoryStream.GetBuffer();
                OutStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public void SaveWrite(string name)
        {
            File.WriteAllBytes(name, GetBytes);
        }
    }
}
