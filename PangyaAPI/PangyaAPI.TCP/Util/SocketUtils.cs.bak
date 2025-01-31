using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PangyaAPI.TCP.Util
{
    public class SocketUtils
    {
        // Definir as constantes para os parâmetros do getsockopt
        private const int SOL_SOCKET = 1;
        private const int SO_CONNECT_TIME = 0x1006; // Código do parâmetro SO_CONNECT_TIME

        // Estrutura para armazenar o tempo de conexão
        [StructLayout(LayoutKind.Sequential)]
        public struct sockaddr
        {
            public short sa_family;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public byte[] sa_data;
        }

        // P/Invoke para chamar o getsockopt
        [DllImport("ws2_32.dll", SetLastError = true)]
        private static extern int getsockopt(IntPtr s, int level, int optname, ref int optval, ref int optlen);

        // Função para obter o tempo de conexão
        public static int GetConnectTime(TcpClient socket)
        {
            // Verifica se o socket é válido
            if (socket == null || !socket.Connected)
            {
                return -1; // _client não conectado
            }

            // Obter o tempo de conexão usando getsockopt
            int seconds = 0;
            int size_seconds = sizeof(int);

            try
            {
                // Chama o getsockopt para obter o tempo de conexão
                int result = getsockopt(socket.Client.Handle, SOL_SOCKET, SO_CONNECT_TIME, ref seconds, ref size_seconds);

                if (result == 0)  // 0 significa sucesso
                {
                    return seconds;  // Retorna o tempo de conexão em segundos
                }
                else
                {
                    return -2;  // Erro ao obter o tempo de conexão
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao obter o tempo de conexão: {ex.Message}");
                return -2;  // Erro ao obter o tempo de conexão
            }
        }
    }
}
