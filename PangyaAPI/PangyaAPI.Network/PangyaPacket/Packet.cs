using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json.Linq;
using PangyaAPI.Network.Cryptor;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;
using uint8_t = System.Byte;

namespace PangyaAPI.Network.PangyaPacket
{

    // Structs auxiliares convertidas de packet.hpp

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class packet_head
    {
        public byte low_key;
        public ushort size;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class packet_head_client : packet_head
    {
        public byte seq;
    }

    public class offset_index
    {
        public byte[] m_buf;
        public ulong m_index_r;
        public ulong m_index_w;
        public ulong m_size;
        public ulong m_size_alloced;

        public void clear() { if (m_buf != null && m_buf.Length > 0) { Array.Clear(m_buf, 0, m_buf.Length); } }
        public void reset_read() => m_index_r = 0;
        public void reset_write()
        {
            m_index_w = 0;
            m_size = 0;
        }
        public void reset()
        {
            reset_read();
            reset_write();
        }
    }

    public class conversionByte
    {
        public const byte CB_BASE_256 = 10;
        public const byte CB_BASE_255 = 20;
        public const byte CB_SEQ_NORMAL = 1;
        public const byte CB_SEQ_INVERTIDA = 2;
        public const byte CB_PARAM_DEFAULT = 0;

        public unionConvertidoStruct unionConvertido;
        private byte m_flag;
        private uint ulNumber_temp;

        public conversionByte()
        {
            unionConvertido = new unionConvertidoStruct();
        }

        public conversionByte(uint _dwConvertido, byte _flag = CB_PARAM_DEFAULT)
        {
            unionConvertido = new unionConvertidoStruct { dwConvertido = _dwConvertido };
            m_flag = _flag;
            if (m_flag != CB_PARAM_DEFAULT) invert();
        }

        public conversionByte(byte[] _ucpConvertido, byte _flag = CB_PARAM_DEFAULT)
        {
            unionConvertido = new unionConvertidoStruct();
            m_flag = _flag;

            if (_ucpConvertido != null && _ucpConvertido.Length >= 4)
                unionConvertido.dwConvertido = BitConverter.ToUInt32(_ucpConvertido, 0);

            if (m_flag != CB_PARAM_DEFAULT)
                invert();
        }

        private void invert()
        {
            if ((m_flag & CB_BASE_255) != 0)
            {
                unionConvertido.dwConvertido = getNumberIS();
                unionConvertido.dwConvertido = getNumberBase256();
            }
            else
            {
                unionConvertido.dwConvertido = getNumberBase255();
                unionConvertido.dwConvertido = getNumberIS();
            }
        }

        public uint getNumberNS() => unionConvertido.dwConvertido;

        public uint getNumberIS()
        {
            return (uint)(unionConvertido.a << 24 | unionConvertido.b << 16 | unionConvertido.c << 8 | unionConvertido.d);
        }

        public byte[] getLPUCNS()
        {
            ulNumber_temp = getNumberNS();
            return BitConverter.GetBytes(ulNumber_temp);
        }

        public byte[] getLPUCIS()
        {
            ulNumber_temp = getNumberIS();
            return BitConverter.GetBytes(ulNumber_temp);
        }

        public uint getNumberBase256() => getNumberNS() * 255 / 256 + 1;
        public uint getNumberBase255() => ((unionConvertido.dwConvertido / 255) << 8) | unionConvertido.dwConvertido % 255;

        public uint getISNumberBase256() => getNumberIS() * 255 / 256 + 1;
        public uint getISNumberBase255() => ((getNumberIS() / 255) << 8) | getNumberIS() % 255;

        public int putNumberBuffer(byte[] buffer)
        {
            if (buffer == null || buffer.Length < 4)
                return -1;

            var bytes = BitConverter.GetBytes(unionConvertido.dwConvertido);
            Array.Copy(bytes, 0, buffer, 0, 4);
            return 4;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct unionConvertidoStruct
        {
            public uint dwConvertido;
            public byte a => (byte)((dwConvertido >> 24) & 0xFF);
            public byte b => (byte)((dwConvertido >> 16) & 0xFF);
            public byte c => (byte)((dwConvertido >> 8) & 0xFF);
            public byte d => (byte)(dwConvertido & 0xFF);
        }
    }

    public class packet : IDisposable
    {
        private MemoryStream _stream;
        /// <summary>
        /// Leitor do packet
        /// </summary>
        private PangyaBinaryReader Reader;
        /// <summary>
        /// Leitor do packet
        /// </summary>
        private PangyaBinaryWriter Reply;
        private bool disposedValue;

        /// <summary>
        /// Mensagem do Packet
        /// </summary>
        public byte[] Decrypt_Msg { get; set; }
        public byte[] Encrypt_Msg { get; set; }
        /// <summary>
        /// Id do Packet
        /// </summary>
        public short Id { get; set; }


        public packet(ushort _id)
        {

            Reply = new PangyaBinaryWriter();

            Reply.init_plain(_id);
        }
        public packet()
        { clear(); }

        public packet(byte[] rawPacket)
        {
            Decrypt_Msg = rawPacket;
            Reply = new PangyaBinaryWriter(new MemoryStream(Decrypt_Msg));
            _stream = new MemoryStream(Decrypt_Msg);
            Reader = new PangyaBinaryReader(_stream);
            Id = ReadInt16();
        }


        /// <summary>
        /// client
        /// </summary>
        /// <param name="rawPacket"></param>
        /// <param name="key"></param>
        public void decrypt_client(byte[] rawPacket, byte key)
        {
            Decrypt_Msg = Cipher.DecryptClient(rawPacket, key);
            _stream = new MemoryStream(Decrypt_Msg);
            Reader = new PangyaBinaryReader(_stream);
            Reply = new PangyaBinaryWriter();
            Debug.WriteLine("packet::decrypt_cliente=>" + Log());
            Id = ReadInt16();
        }


        public void decrypt_server(byte[] rawPacket, byte key)
        {
            Decrypt_Msg = Cipher.DecryptClient(rawPacket, key);
            _stream = new MemoryStream(Decrypt_Msg);
            Reader = new PangyaBinaryReader(_stream);
            Reply = new PangyaBinaryWriter();
            Debug.WriteLine("packet::decrypt_server=>" + Log());
            Id = Reader.ReadInt16();
        }

        public void encrypt(byte[] rawPacket, byte key)
        {
            Encrypt_Msg = Cipher.ServerEncrypt(rawPacket, key, 0);
            Reply = new PangyaBinaryWriter();
        }

        public void encrypt(byte key)
        {
            Encrypt_Msg = Cipher.ServerEncrypt(Reply.GetBytes, key, 0);
            Reply = new PangyaBinaryWriter();
        }

        public void setRaw(byte[] rawPacket)
        {

            Decrypt_Msg = rawPacket;
            _stream = new MemoryStream(Decrypt_Msg);

            Reader = new PangyaBinaryReader(_stream);
            Id = ReadInt16();
        }



        public void init_plain(ushort value)
        {
            if (Reply == null)
                Reply = new PangyaBinaryWriter();

            Reply.init_plain(value);
        }

        public uint GetSize
        {
            get => Reader.Size;
        }

        public byte[] getBuffer()
        {
            Encrypt_Msg = Reply.GetBytes;

            return (Encrypt_Msg);
        }

        public short getTipo() => Id;
        public uint GetPos
        {
            get => Reader.GetPosition();
        }

        public double ReadDouble()
        {
            return Reader.ReadDouble();
        }

        public byte ReadUInt8()
        {
            return Reader.ReadByte();
        }

        public short ReadInt16()
        {
            return Reader.ReadInt16();
        }
        public ushort ReadUInt16()
        {
            return Reader.ReadUInt16();
        }
        public uint[] ReadUInt32(uint size)
        {
            return Reader.Read(size).ToArray();
        }
        public uint ReadUInt32()
        {
            return Reader.ReadUInt32();
        }
        public int[] ReadInt32(int size)
        {
            return Reader.Read(size).ToArray();
        }
        public int ReadInt32()
        {
            return Reader.ReadInt32();
        }

        public ulong ReadUInt64()
        {
            return Reader.ReadUInt64();
        }

        public long ReadInt64()
        {
            return Reader.ReadInt64();
        }

        public float ReadSingle()
        {
            return Reader.ReadSingle();
        }

        public string ReadString()
        {
            return Reader.ReadPStr();
        }
        public void Skip(int count)
        {
            Reader.Skip(count);
        }


        public void Seek(int offset, int origin)
        {
            Reader.Seek(offset, origin);
        }

        public T ReadStruct<T>()
        {
            return Reader.ReadStruct<T>();
        }

        public T Read<T>() where T : new()
        {
            return Reader.Read<T>();
        }


        public IEnumerable<uint> Read(uint count)
        {
            return Reader.Read(count);
        }
        public object Read(object value, int Count)
        {
            return Reader.Read(value, Count);
        }

        public object Read(object value)
        {
            return Reader.Read(value);
        }



        public string ReadPStr(uint Count)
        {
            var data = new byte[Count];
            //ler os dados
            Reader.BaseStream.Read(data, 0, (int)Count);
            var value = Encoding.ASCII.GetString(data);
            return value;
        }

        public bool ReadPStr(out string value, uint Count)
        {
            return Reader.ReadPStr(out value, Count);
        }
        public bool ReadPStr(out string value)
        {
            return Reader.ReadPStr(out value);
        }
        public string ReadPStr()
        {
            return Reader.ReadPStr();
        }
        public bool ReadDouble(out Double value)
        {
            return Reader.ReadDouble(out value);
        }
        public bool ReadBytes(out byte[] value)
        {
            return Reader.ReadBytes(out value);
        }

        public bool ReadBytes(out byte[] value, int len)
        {
            return Reader.ReadBytes(out value, len);
        }

        public bool ReadBytes(ref byte[] value, uint len)
        {
            Reader.ReadBytes(out value, (int)len);
            return true;
        }
        public bool ReadByte(out byte value)
        {
            return Reader.ReadByte(out value);
        }
        public byte ReadByte()
        {
            return Reader.ReadByte();
        }
        public bool ReadInt16(out short value)
        {
            return Reader.ReadInt16(out value);
        }
        public bool ReadUInt16(out ushort value)
        {
            return Reader.ReadUInt16(out value);
        }

        public bool ReadUInt32(out uint value)
        {
            return Reader.ReadUInt32(out value);
        }

        public bool ReadInt32(out int value)
        {
            return Reader.ReadInt32(out value);
        }

        public bool ReadUInt64(out ulong value)
        {
            return Reader.ReadUInt64(out value);
        }

        public bool ReadInt64(out long value)
        {
            return Reader.ReadInt64(out value);
        }

        public bool ReadSingle(out float value)
        {
            return Reader.ReadSingle(out value);
        }

        public sbyte ReadSByte()
        {
            return Reader.ReadSByte();
        }

        public void ReadBuffer<T>(ref T value, int size)
        {
            Reader.ReadBuffer(ref value, size);
        }

        public float ReadFloat()
        {
            return Reader.ReadSingle();
        }


        public byte[] GetRemainingData
        {
            get => Reader.GetRemainingData();
        }

        public byte[] ReadBytes(int count)
        {
            return Reader.ReadBytes(count);
        }


        public void SetReader(PangyaBinaryReader read)
        {
            Reader = read;
        }
 
        public void Version_Decrypt(ref uint packetVer)
        {
           string PacketVerKey = "{873AE210-2EEF-4c61-B030-A54F17634A7D}";

            byte[] tmpPVer = BitConverter.GetBytes(packetVer);
            int index = 0;

            for (int i = 0; i < PacketVerKey.Length; i++)
            {
                tmpPVer[index] ^= (byte)PacketVerKey[i];
                index = (index == 3) ? 0 : index + 1;
            }

            packetVer = BitConverter.ToUInt32(tmpPVer, 0);
        }

        public string Log()
        {
            return Decrypt_Msg.HexDump();
        }

        public void makeRaw()
        {
            packet_head ph = new packet_head();

            if (Reply.GetBytes == null)
            {
                throw new exception("Error buf is nullptr em packet::makeRaw()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET,
                    15, 0));
            }

            ph.low_key = 0; // low part of key random - 0 nesse pacote porque ele é o primiero que passa a chave
            ph.size = (ushort)(Reply.GetBytes.Length + 1);

            var m_maked = Reply.GetBytes;
            // Maked Reset
            Reply.Clear();

            Reply.WriteBuffer(ph, 3);
            Reply.WriteByte(0);// byte com valor 0 para dizer que é um pacote raw
            Reply.WriteBytes(m_maked);
        }

        public void clear()
        {
            Reply = new PangyaBinaryWriter();
            _stream = new MemoryStream();
            Reader = new PangyaBinaryReader(_stream);
            Decrypt_Msg = new byte[0];
            Encrypt_Msg = new uint8_t[0];
        }
         

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: descartar o estado gerenciado (objetos gerenciados)
                    if (Reply != null)
                        Reply.Dispose();
                    if (Reader != null)
                        Reader.Dispose();
                    if (_stream != null)
                        _stream.Dispose();
                    Encrypt_Msg = new uint8_t[0];
                    Decrypt_Msg = new uint8_t[0];
                }

                // TODO: liberar recursos não gerenciados (objetos não gerenciados) e substituir o finalizador
                // TODO: definir campos grandes como nulos
                disposedValue = true;
            }
        }

        // // TODO: substituir o finalizador somente se 'Dispose(bool disposing)' tiver o código para liberar recursos não gerenciados
        // ~packet()
        // {
        //     // Não altere este código. Coloque o código de limpeza no método 'Dispose(bool disposing)'
        //     Dispose(disposing: false);
        // }

        void IDisposable.Dispose()
        {
            // Não altere este código. Coloque o código de limpeza no método 'Dispose(bool disposing)'
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void AddByte(byte value)
        {
            Reply.WriteByte(value);
        }
    }
}
