using System;
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

        public PangyaBinaryWriter() : base(new MemoryStream())
        {
            //this.OutStream = new MemoryStream();
            //this._Encoding = Encoding.GetEncoding("Shift_JIS"); // Japan!         
        }
        public PangyaBinaryWriter(ushort id) : base(new MemoryStream())
        {
            init_plain(id);
        }
        public PangyaBinaryWriter(short id) : base(new MemoryStream())
        {
            WriteInt16(id);
        }
        public uint GetSize => (uint)BaseStream.Length;
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

        public bool WriteBytes(byte[] message)
        {
            try
            {                          
              return  WriteBytes(message, message.Length);
            }
            catch
            {
                return false;
            }
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
        public bool WriteSByte(sbyte value)
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

        public bool WriteUInt32(uint[] values)
        {
            try
            {
                for (uint i = 0; i < values.Count(); i++)      
                Write(values[i]);
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

        public bool WriteStruct<T>(T value) where T : struct
        {
            try
            {
                // Certifique-se de que o tipo T é realmente uma estrutura.
                if (!typeof(T).IsValueType)
                    throw new ArgumentException("O parâmetro 'value' deve ser uma estrutura.");

                int size = Marshal.SizeOf(typeof(T));
                byte[] buffer = new byte[size];

                IntPtr ptr = Marshal.AllocHGlobal(size);
                try
                {
                    Marshal.StructureToPtr(value, ptr, false);
                    Marshal.Copy(ptr, buffer, 0, size);

                    // Método Write recebe buffer e tamanho.
                    Write(buffer, size);
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WriteStruct][Error] Ocorreu um erro ao serializar a estrutura: {ex.Message}");
                Console.WriteLine($"Pilha de chamada: {ex.StackTrace}");
                return false;
            }

            return true;
        }

        public void WriteFile(string file)
        {
            File.WriteAllBytes(file, GetBytes);
        }

        public bool WriteStruct(object value, object valueOri)
        {
            if (value == null || valueOri == null)
            {
                Console.WriteLine("[WriteStruct][Warning] Parâmetros 'value' ou 'valueOri' são nulos.");
                return false;
            }

            try
            {
                int size = Marshal.SizeOf(valueOri);

                // Aloca memória e copia os dados da estrutura
                IntPtr ptr = Marshal.AllocHGlobal(size);
                try
                {
                    Marshal.StructureToPtr(value, ptr, true);
                    byte[] buffer = new byte[size];
                    Marshal.Copy(ptr, buffer, 0, size);

                    // Chama o método Write para manipular os dados
                    Write(buffer, size);
                }
                catch (ArgumentException ex) // Corrigido para Exception com "E" maiúsculo
                {
                    // Log estruturado do erro
                    Console.WriteLine("[WriteStruct][Error] Ocorreu um erro ao manipular a estrutura:");
                    Console.WriteLine($"Mensagem: {ex.Message}");
                    Console.WriteLine($"Origem: {ex.Source}");
                    Console.WriteLine($"Método: {ex.TargetSite}");
                    Console.WriteLine($"Pilha de chamada: {ex.StackTrace}");
                    return false;
                }
                finally
                {
                    // Libera a memória alocada
                    Marshal.FreeHGlobal(ptr);
                }
            }
            catch (Exception ex) // Corrigido para Exception com "E" maiúsculo
            {
                // Log estruturado do erro
                Console.WriteLine("[WriteStruct][Error] Ocorreu um erro ao manipular a estrutura:");
                Console.WriteLine($"Mensagem: {ex.Message}");
                Console.WriteLine($"Origem: {ex.Source}");
                Console.WriteLine($"Método: {ex.TargetSite}");
                Console.WriteLine($"Pilha de chamada: {ex.StackTrace}");
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
