using PangyaAPI.Network.Cryptor;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace PangyaAPI.Network.PangyaSession
{
    public abstract partial class SessionBase
    {
        private readonly Channel<byte[]> _queue = Channel.CreateUnbounded<byte[]>(); // Fila otimizada
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1); // Garante apenas uma Task de envio
        private volatile bool _isSending = false; // Flag para indicar envio ativo
 
        public virtual void Send(byte[] data, bool debug_log = true)
        {
            if (debug_log)
                Console.WriteLine("[SessionBase::Send][HexLog]: " + data.HexDump());

            if (m_key != 255) // Encrypt se necessário
                data = data.ServerEncrypt(m_key, 0);

            _queue.Writer.TryWrite(data); // Adiciona à fila sem bloquear
        }

        public virtual void Send(PangyaBinaryWriter packet, bool debug_log = true)
        {
            if (debug_log)
                Console.WriteLine("[SessionBase::Send][HexLog]: " + packet.GetBytes.HexDump() + Environment.NewLine);

            if (m_key != 255)
                _queue.Writer.TryWrite(packet.GetBytes.ServerEncrypt(m_key, 0));

        }


        private async void StartSendingLoop()
        {
            if (_isSending) return;
            _isSending = true;

            try
            {
                List<byte[]> batch = new List<byte[]>(10); // Buffer para envio em lote
                while (await _queue.Reader.WaitToReadAsync())
                {
                    batch.Clear();

                    // Pega até 10 pacotes por vez para envio mais eficiente
                    while (batch.Count < 10 && _queue.Reader.TryRead(out byte[] packet))
                        batch.Add(packet);

                    if (batch.Count > 0 && getConnected())
                        await SendBatchAsync(batch);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no loop de envio: {ex.Message}");
            }
            finally
            {
                _isSending = false;
            }
        }

        private async Task SendBatchAsync(List<byte[]> packets)
        {
            try
            {
                NetworkStream stream = _client?.GetStream();
                if (stream == null || !stream.CanWrite) return;

                // Calcula tamanho total do buffer
                int totalSize = 0;
                foreach (var packet in packets)
                    totalSize += packet.Length;

                // Usa um buffer reutilizável para evitar alocações excessivas
                byte[] buffer = ArrayPool<byte>.Shared.Rent(totalSize);
                try
                {
                    int offset = 0;
                    foreach (var packet in packets)
                    {
                        Buffer.BlockCopy(packet, 0, buffer, offset, packet.Length);
                        offset += packet.Length;
                    }

                    // Envio único do buffer otimizado
                    await stream.WriteAsync(buffer, 0, totalSize);
                    await stream.FlushAsync();
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Erro de escrita no stream: {ex.Message}");
                Disconnect();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar pacotes: {ex.Message}");
            }
        }
    }
}
