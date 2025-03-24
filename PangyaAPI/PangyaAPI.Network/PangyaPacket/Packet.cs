using PangyaAPI.Utilities.BinaryModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System;
using PangyaAPI.Utilities;
using System.Runtime.InteropServices;

namespace PangyaAPI.Network.PangyaPacket
{
    public class Packet
    {

        #region Private Fields
        private readonly MemoryStream _stream;
        /// <summary>
        /// Leitor do packet
        /// </summary>
        private PangyaBinaryReader Reader;
        /// <summary>
        /// Mensagem do Packet
        /// </summary>
        public byte[] Message { get; set; }
        #endregion

        #region Public Fields
        /// <summary>
        /// Id do Packet
        /// </summary>
        public short Id { get; set; }
        #endregion

        #region Constructor

        public Packet(byte[] rawPacket, byte key)
        {
            Message = Cryptor.Cipher.DecryptClient(rawPacket, key);

            _stream = new MemoryStream(Message);
            Reader = new PangyaBinaryReader(_stream);
            Id = ReadInt16();

            // Exibindo o título com o ID do pacote
            Console.Write($"[{UtilTime.FormatDate(DateTime.Now)}] [Packet::Packet{Id:X}, HexDump: ");

            // Imprime os bytes em formato hexadecimal
            for (int i = 0; i < Message.Length; i++)
            {
                // Imprime os bytes com 2 caracteres em formato hexadecimal
                Console.Write($"{Message[i]:X2} ");

                // A cada 16 bytes, adiciona uma quebra de linha para tornar a saída mais legível
                if ((i + 1) % 16 == 0)
                {
                    Console.WriteLine(); // Quebra de linha após 16 bytes
                }
            }

            // Adiciona a parte final da string "]" e uma quebra de linha
            Console.WriteLine("]"); // Fecha o hex dump
        }


        public Packet(byte[] rawPacket)
        {

            Message = rawPacket;
            _stream = new MemoryStream(Message);

            Reader = new PangyaBinaryReader(_stream);
            Id = ReadInt16();
        }

        #endregion

        #region Methods Get
        public uint GetSize
        {
            get => Reader.Size;
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

        #endregion

        public string Log()
        {
            return " [HexDump:" + Message.HexDump() + "]";
        }

        public void Version_Decrypt(ref uint @packet_version)
        {
            @packet_version = Cryptor.Cipher.DecryptClient(@packet_version);
        }

        public sbyte ReadSByte()
        {
            return Reader.ReadSByte();
        }
    }
}
