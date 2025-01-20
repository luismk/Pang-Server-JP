using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PangyaAPI.TCP.Util
{
    public class PangyaBuffer
    {
        private const int MAX_BUFFER_SIZE = 8192; // Defina o tamanho máximo do buffer conforme necessário
        private byte[] m_buffer;
        private int m_index_w;
        private int m_sequence;
        private int m_seq_mode;
        private int m_operation;
        private static int static_sequence = 0;
        private object m_cs_wr;

        public PangyaBuffer()
        {
            Clear();
            m_cs_wr = new object();
        }

        public PangyaBuffer(int seq_mode) : this()
        {
            m_seq_mode = seq_mode;
            if (m_seq_mode == 1)
            {
                m_sequence = (int)Interlocked.Increment(ref static_sequence) - 1;
            }
        }

        public PangyaBuffer(byte[] buffer, int seq_mode = 1) : this(seq_mode)
        {
            Write(buffer, buffer.Length);
        }

        public void Clear()
        {
            m_index_w = 0;
            m_sequence = 0;
            m_seq_mode = 0;
            m_operation = -1;
            m_buffer = new byte[MAX_BUFFER_SIZE];
        }

        public void Init(int seq_mode)
        {
            m_seq_mode = seq_mode;
            if (m_seq_mode == 1)
            {
                m_sequence = (int)Interlocked.Increment(ref static_sequence) - 1;
            }
        }

        public void Init(byte[] buffer, int seq_mode)
        {
            m_seq_mode = seq_mode;
            if (m_seq_mode == 1)
            {
                m_sequence = (int)Interlocked.Increment(ref static_sequence) - 1;
            }
            Write(buffer, buffer.Length);
        }

        public void Reset()
        {
            lock (m_cs_wr)
            {
                m_index_w = 0;
            }
        }

        public int AddSize(int size)
        {
            lock (m_cs_wr)
            {
                if ((size + m_index_w) > MAX_BUFFER_SIZE)
                {
                    return MAX_BUFFER_SIZE - (size + m_index_w);
                }

                int sz_write = (int)CheckSize(size);
                m_index_w += sz_write;
                return sz_write;
            }
        }

        public int Write(byte[] buffer, int size)
        {
            if (buffer == null)
                throw new Exception("Erro: buffer is null. Buffer::write()");

            lock (m_cs_wr)
            {
                int sz_write = (int)CheckSize(size);
                if (sz_write <= 0)
                    return 0;

                Array.Copy(buffer, 0, m_buffer, m_index_w, sz_write);
                m_index_w += sz_write;
                return sz_write;
            }
        }

        public int Read(byte[] buffer, int size)
        {
            if (buffer == null)
                throw new Exception("Erro: buffer is null. Buffer::read()");

            lock (m_cs_wr)
            {
                if (size > m_index_w)
                    return m_index_w;

                Array.Copy(m_buffer, 0, buffer, 0, size);
                m_index_w -= size;

                if (m_index_w > 0)
                    Array.Copy(m_buffer, size, m_buffer, 0, m_index_w);

                return size;
            }
        }

        public int Peek(byte[] buffer, int size)
        {
            if (buffer == null)
                throw new Exception("Erro: buffer is null. Buffer::peek()");

            lock (m_cs_wr)
            {
                if (size > m_index_w)
                    return m_index_w;

                Array.Copy(m_buffer, 0, buffer, 0, size);
                return size;
            }
        }

        public void Consume(int size)
        {
            lock (m_cs_wr)
            {
                if (m_index_w <= 0)
                    throw new Exception("Erro nao pode consumir mais, o buffer index é menor ou igual a 0. Buffer::consume()");

                if ((m_index_w - size) > MAX_BUFFER_SIZE)
                    throw new Exception("Erro nao pode consumir mais, o tamanho requisitado é maior que o buffer index. Buffer::consume()");

                m_index_w -= size;

                if (m_index_w > 0)
                    Array.Copy(m_buffer, size, m_buffer, 0, m_index_w);
            }
        }

        private long CheckSize(long size)
        {
            return (size + m_index_w) <= MAX_BUFFER_SIZE ? size : MAX_BUFFER_SIZE - m_index_w;
        }

        public byte[] GetBuffer()
        {
            return m_buffer;
        }

        public int GetSize()
        {
            return MAX_BUFFER_SIZE;
        }

        public int GetUsed()
        {
            return m_index_w;
        }

        public void SetOperation(int operation)
        {
            m_operation = operation;
        }

        public int GetOperation()
        {
            return m_operation;
        }

        public int GetSequence()
        {
            return m_sequence;
        }

        public bool IsOrder()
        {
            return m_seq_mode == 1;
        }

        public void SetOverlapped()
        {
            Array.Clear(m_buffer, 0, m_buffer.Length);
        }

        public byte[] GetWSABufToRead()
        {
            lock (m_cs_wr)
            {
                byte[] buf = new byte[MAX_BUFFER_SIZE - m_index_w];
                Array.Copy(m_buffer, m_index_w, buf, 0, buf.Length);
                return buf;
            }
        }

        public byte[] GetWSABufToSend()
        {
            lock (m_cs_wr)
            {
                byte[] buf = new byte[m_index_w];
                Array.Copy(m_buffer, 0, buf, 0, buf.Length);
                return buf;
            }
        }   
    }
}
