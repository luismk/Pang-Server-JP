using System;
using System.Collections.Concurrent;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using PangyaAPI.Network.PangyaUtil;

namespace PangyaAPI.Network.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SOCKADDR_IN
    {
        public short sin_family;
        public ushort sin_port;
        public uint sin_addr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] sin_zero;
    }

    public static class Interop
    {
        public const int SD_BOTH = 2;
        public const int SOCKET_ERROR = -1;
        public static readonly IntPtr INVALID_SOCKET = new IntPtr(-1);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        [DllImport("libc")]
        public static extern int gettid();
        [DllImport("Ws2_32.dll", SetLastError = true)]
        public static extern int shutdown(IntPtr s, int how);

        [DllImport("Ws2_32.dll", SetLastError = true)]
        public static extern int closesocket(IntPtr s);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetEvent(IntPtr hEvent);

        [DllImport("Ws2_32.dll", SetLastError = true)]
        public static extern int setsockopt(IntPtr s, int level, int optname, IntPtr optval, int optlen);

        [DllImport("Ws2_32.dll", SetLastError = true)]
        public static extern int getsockname(IntPtr s, IntPtr name, ref int namelen);

        [DllImport("Ws2_32.dll", SetLastError = true)]
        public static extern int getpeername(IntPtr s, IntPtr name, ref int namelen);

        [DllImport("Ws2_32.dll")]
        public static extern int WSAGetLastError();

        public const int SOL_SOCKET = 0xffff;
        public const int SO_UPDATE_ACCEPT_CONTEXT = 0x700B;

        public const int SOCKADDR_IN_SIZE = 16;

        [DllImport("Ws2_32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr inet_ntop(int af, IntPtr src, [Out] byte[] dst, int size);

        public static string GetIpFromSockaddr(IntPtr sockaddr)
        {
            byte[] buffer = new byte[64];
            IntPtr result = inet_ntop(2, sockaddr + 4, buffer, buffer.Length);
            if (result == IntPtr.Zero)
                return "Invalid IP";
            return System.Text.Encoding.ASCII.GetString(buffer).TrimEnd('\0');
        }

        public static uint GetIpFromSockaddrAsUInt(IntPtr sockaddr)
        {
            byte[] bytes = new byte[4];
            Marshal.Copy(sockaddr + 4, bytes, 0, 4);
            return BitConverter.ToUInt32(bytes, 0);
        }
        public static IPAddress PtrToIPAddress(IntPtr ptr)
        {
            var addr = Marshal.PtrToStructure<SOCKADDR_IN>(ptr);
            byte[] ipBytes = BitConverter.GetBytes(addr.sin_addr);
            if (BitConverter.IsLittleEndian) Array.Reverse(ipBytes);
            var ip = new IPAddress(ipBytes);
            return ip;
        }
        public static IPEndPoint PtrToIPEndPoint(IntPtr ptr)
        {
            var addr = Marshal.PtrToStructure<SOCKADDR_IN>(ptr);

            // IP
            byte[] ipBytes = BitConverter.GetBytes(addr.sin_addr);
            if (BitConverter.IsLittleEndian) Array.Reverse(ipBytes);
            var ip = new IPAddress(ipBytes);

            // Porta (em network byte order → host)
            ushort port = (ushort)IPAddress.NetworkToHostOrder((short)addr.sin_port);

            return new IPEndPoint(ip, port);
        }

    }

    public class myOver
    {
        public PangyaBuffer buffer;
        public uint tipo;
        public myOver(PangyaBuffer _buffer, uint _tipo)
        {
            this.buffer = _buffer;
            this.tipo = _tipo;
        }

        public static explicit operator myOver(PangyaBuffer v)
        {
            return new myOver(v, v.getOperation());
        }
    }
    public class iocp
    {
        private readonly BlockingCollection<WorkItem> m_queue = new BlockingCollection<WorkItem>();
        private readonly int maxThreads;

        public iocp(int max_num_thread_same_time = 16)
        {
            maxThreads = max_num_thread_same_time;

            for (int i = 0; i < maxThreads; i++)
            {
                Task.Factory.StartNew(WorkerLoop, TaskCreationOptions.LongRunning);
            }
        }

        public void postStatus(PangyaSession.Session completionKey, uint numBytes, object state)
        {
            m_queue.Add(new WorkItem
            {
                CompletionKey = completionKey,
                NumBytes = numBytes,
                State = state
            });
        }

        public void getStatus(out PangyaSession.Session completionKey, out uint numBytes, out object state, int timeoutMillis = Timeout.Infinite)
        {
            if (m_queue.TryTake(out WorkItem item, timeoutMillis))
            {
                completionKey = item.CompletionKey;
                numBytes = item.NumBytes;
                state = item.State;
            }
            else
            {
                throw new TimeoutException("Timeout ao obter status.");
            }
        }

        private void WorkerLoop()
        {
            foreach (var item in m_queue.GetConsumingEnumerable())
            {
                // Aqui você trataria a operação da thread
                // Ex: translate_operation(...)
                Console.WriteLine($"Processando item: Key={item.CompletionKey}, Bytes={item.NumBytes}");
            }
        }

        public bool associaDeviceToPort(object completionKey, object device)
        {
            // Em C# puro isso é simbólico — poderia registrar um mapeamento, se necessário
            return true;
        }

        private class WorkItem
        {
            public PangyaSession.Session CompletionKey { get; set; }
            public uint NumBytes { get; set; }
            public object State { get; set; }
        }
    }
}
