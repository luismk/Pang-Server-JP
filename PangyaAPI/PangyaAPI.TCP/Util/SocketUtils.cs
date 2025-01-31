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
    // Constantes necessárias para getsockopt
    private const int SOL_SOCKET = 0xFFFF;
    private const int SO_CONNECT_TIME = 0x700C;

    [DllImport("ws2_32.dll", SetLastError = true)]
    private static extern int getsockopt(IntPtr s, int level, int optname, out int optval, ref int optlen);

    // Função para obter o tempo de conexão
    public static int GetConnectTime(this TcpClient socket)
    {
        if (socket == null || !socket.Connected)
        {
            return -1; // Cliente não está conectado
        }

        int seconds;
        int size_seconds = sizeof(int);

        try
        {
            // Chama getsockopt para obter o tempo de conexão
            int result = getsockopt(socket.Client.Handle, SOL_SOCKET, SO_CONNECT_TIME, out seconds, ref size_seconds);

            if (result == 0)  // Sucesso
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
            return -2;
        }
    }
}
