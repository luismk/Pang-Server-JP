using System;
using System.Runtime.InteropServices;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.Utilities;

namespace PangyaAPI.Network.PangyaUtil
{
    public class PangyaBuffer
    {
        private const int MAX_BUFFER_SIZE = 0x4000;
        private static volatile uint static_sequence = 0;

        private uint m_sequence;
        private int m_seq_mode;
        private byte[] m_buffer;
        private int m_index_w;
        private object m_cs_wr = new object();
        private _WSABUF m_mwsab;
        private uint m_operation;

        public PangyaBuffer()
        {
            init(0);
        }

        public PangyaBuffer(int seq_mode)
        {
            init(seq_mode);
        }

        public PangyaBuffer(byte[] buffer, int size, int seq_mode)
        {
            init(buffer, size, seq_mode);
        }

        public PangyaBuffer(byte[] buffer, int seq_mode)
        {
            init(buffer, buffer.Length, seq_mode);
        }

        public void clear()
        {
            lock (m_cs_wr)
            {
                Array.Clear(m_buffer, 0, m_buffer.Length);
                m_index_w = 0;
            }
        }

        public void init(int seq_mode)
        {
            m_seq_mode = seq_mode;
            m_buffer = new byte[MAX_BUFFER_SIZE];
            m_index_w = 0;
            m_sequence = static_sequence++;
            m_mwsab = new _WSABUF();
        }

        public void init(byte[] buffer, int size, int seq_mode)
        {
            if (buffer == null || size < 0 || size > MAX_BUFFER_SIZE)
                throw new ArgumentException("Buffer inválido ou tamanho fora do limite.");

            init(seq_mode);
            Buffer.BlockCopy(buffer, 0, m_buffer, 0, size);
            m_index_w = size;
        }

        public void reset()
        {
            lock (m_cs_wr)
            {
                m_index_w = 0;
            }
        }

        public int addSize(uint size)
        {
            lock (m_cs_wr)
            {
                if ((size + m_index_w) > MAX_BUFFER_SIZE)
                {
                    return (int)(MAX_BUFFER_SIZE - (size + m_index_w));
                }

                int sz_write = checkSize((int)size);

                if (sz_write < 0)
                    sz_write = 0;

                m_index_w += sz_write;

                return sz_write;
            }
        }


        public int write(byte[] buffer, int size)
        {
            if (buffer == null)
                throw new exception("Erro: buffer is null. Buffer::write()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.BUFFER, 1, 0));


            lock (m_cs_wr)
            {
                if (size > MAX_BUFFER_SIZE - m_index_w)
                    return -1;

                Buffer.BlockCopy(buffer, 0, m_buffer, m_index_w, size);
                m_index_w += size;
                return size;
            }
        }



        public int Write(byte[] buffer, int size)
        {
            if (buffer == null)
                throw new Exception("Erro: buffer is null. Buffer::Write()");

            lock (m_cs_wr)
            {
                int sz_write = checkSize(size);

                if (sz_write <= 0)
                    return 0;

                // Copia os dados para o buffer interno
                Buffer.BlockCopy(buffer, 0, m_buffer, m_index_w, sz_write);

                m_index_w += sz_write;

                return sz_write;
            }
        }

        public int checkSize(int size)
        {
            if ((size + m_index_w) <= MAX_BUFFER_SIZE)
                return size;
            else
                return (int)(MAX_BUFFER_SIZE - m_index_w);
        }

        public int read(byte[] buffer, uint size)
        {
            if (buffer == null)
                throw new exception("Erro: buffer is null. Buffer::read()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.BUFFER, 1, 0));

            lock (m_cs_wr)
            {
                if (size > m_index_w)
                    return -1;

                Buffer.BlockCopy(m_buffer, 0, buffer, 0, (int)size);
                consume(size);
                return (int)size;
            }
        }

        public int peek(ref byte[] buffer, int size)
        {
            if (buffer == null)
                throw new exception("Erro: buffer is null. Buffer::peek()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.BUFFER, 1, 0));

            lock (m_cs_wr)
            {
                if (size > m_index_w)
                    return -1;

                Buffer.BlockCopy(m_buffer, 0, buffer, 0, size);
                return size;
            }
        }

        public T peek<T>(int size) where T : new()
        {
            T local;
            byte[] buffer = new byte[size];

            if (buffer == null)
                throw new exception("Erro: buffer is null. Buffer::peek()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.BUFFER, 1, 0));

            lock (m_cs_wr)
            {
                if (size > m_index_w)
                    return default;

                Buffer.BlockCopy(m_buffer, 0, buffer, 0, size);
                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                local = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            return local;
        }

        public void consume(uint size)
        {
            if (m_index_w <= 0)
                throw new exception("Erro nao pode consumir mais, o buffer index is less or equal 0. Buffer::consume()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.BUFFER, 2, 0));

            else if ((m_index_w - size) > MAX_BUFFER_SIZE/*Por que é unsigned ai negativo fica o maximo do int32_t que é maior que o MAX_BUFFER_SIZE*/)
                throw new exception("Erro nao pode consumir mais, o tamanho requisitado e maior que o buffer index. Buffer::consume()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.BUFFER, 2, 0));

            lock (m_cs_wr)
            {
                if (size > m_index_w)
                    size = (uint)m_index_w;

                Buffer.BlockCopy(m_buffer, (int)size, m_buffer, 0, m_index_w - (int)size);
                m_index_w -= (int)size;
            }
        }

        public byte[] getBuffer()
        {
            return m_buffer;
        }

        public int getSize()
        {
            return m_buffer.Length;
        }

        public int getUsed()
        {
            return m_index_w;
        }

        public _WSABUF getWSABufToRead()
        {
            if (m_mwsab == null)
                throw new InvalidOperationException("WSABUF não foi inicializado.");

            Buffer.BlockCopy(m_buffer, 0, m_mwsab.buf, 0, m_index_w);


            if (m_index_w > MAX_BUFFER_SIZE)    // chegou ao limite enviar recive de 0 bytes
                m_mwsab.len = 0;
            else
                m_mwsab.len = (uint)(MAX_BUFFER_SIZE - m_index_w);
            return m_mwsab;
        }

        public _WSABUF getWSABufToSend()
        {
            if (m_mwsab == null)
                throw new InvalidOperationException("WSABUF não foi inicializado.");

            m_mwsab.buf = m_buffer.memcpy(m_index_w);
            m_mwsab.len = (uint)m_index_w;
            return m_mwsab;
        }

        public uint getOperation()
        {
            return m_operation;
        }

        public OP_TYPE getOperation_Type()
        {
            return (OP_TYPE)m_operation;
        }

        public void setOperation(uint _operation)
        {
            m_operation = _operation;
        }

        public uint getSequence()
        {
            return m_sequence;
        }

        public bool isOrder()
        {
            return m_seq_mode != 0;
        }

        public packet getPacket()
        {
            return new packet(getWSABufToSend().buf);
        }
    }
}
