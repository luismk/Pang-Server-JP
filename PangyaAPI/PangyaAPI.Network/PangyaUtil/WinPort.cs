using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using PangyaAPI.Network.PangyaSession;

namespace PangyaAPI.Network.PangyaUtil
{
    public class SOCKET : TcpClient, IDisposable
    {
        public int fd = new int();
        public TimeSpec connect_time = new TimeSpec();

        public SOCKET()
        {
        }

        public SOCKET(IPEndPoint localEP) : base(localEP)
        {
        }

        public SOCKET(AddressFamily family) : base(family)
        {
        }

        public SOCKET(string hostname, int port) : base(hostname, port)
        {
        }
    }
    public class stThreadpoolMessage
    {
        public stThreadpoolMessage(uint _ul = 0u)
        {
            this._session = null;
            this.buffer = (null);
            this.dwIOsize = 0u;
        }
        public stThreadpoolMessage(Session __session,
            PangyaBuffer _buffer, uint _dwIOsize)
        {
            this._session = __session;
            this.buffer = (_buffer);
            this.dwIOsize = _dwIOsize;
        }

        public Session _session;
        public PangyaBuffer buffer;
        public uint dwIOsize;
    }
    public class _WSABUF
    {
        public uint len = new uint();
        public byte[] buf = new byte[8196];

        [DllImport("Ws2_32.dll", SetLastError = true)]
        public static extern int WSARecv(
    IntPtr socket,
    [In, Out] _WSABUF buffers,
    int bufferCount,
    IntPtr bytesTransferred,
    ref uint flags,
    out IntPtr overlapped,
    IntPtr completionRoutine
);

    }
    [StructLayout(LayoutKind.Explicit)]
    public struct LARGE_INTEGER
    {
        [FieldOffset(0)]
        public ulong QuadPart;

        [FieldOffset(0)]
        public uint LowPart;

        [FieldOffset(4)]
        public uint HighPart;

        // Opcional: pode adicionar propriedades para facilitar o uso
        public void SetFromParts(uint low, uint high)
        {
            LowPart = low;
            HighPart = high;
        }

        public (uint Low, uint High) GetParts()
        {
            return (LowPart, HighPart);
        }
    }

    public struct TimeSpec
    {
        public long tv_sec;
        public long tv_nsec;

        public TimeSpec(long ticks, long frequency)
        {
            tv_sec = ticks / frequency;
            tv_nsec = (ticks % frequency) * 1_000_000_000 / frequency;
        }

        public override string ToString() => $"{tv_sec}s {tv_nsec}ns";

        [DllImport("kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        public static TimeSpec GetTick()
        {
            QueryPerformanceCounter(out long counter);
            QueryPerformanceFrequency(out long frequency);

            return new TimeSpec(counter, frequency);
        }
    }
}
