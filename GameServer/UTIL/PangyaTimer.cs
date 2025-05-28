using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
using Pangya_GameServer.Game.Base;
using PangyaAPI.Utilities.Log;

namespace Pangya_GameServer.UTIL
{
    public class ThreadManager : IDisposable
    {
        private Thread _thread;
        public string Name { get => _thread.Name; set => _thread.Name = value; }
        public bool IsBackground { get => _thread.IsBackground; set => _thread.IsBackground = value; }
        private ManualResetEventSlim _waitStartEvent = new ManualResetEventSlim(false);
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public ThreadManager(Action<CancellationToken> threadStart)
        {
            // Cria a thread que executa a ação fornecida
            _thread = new Thread(() => threadStart(_cts.Token))
            {
                IsBackground = true
            };
            _thread.Start();
        }

        public ThreadManager(Thread threadStart)
        {
            threadStart.IsBackground = true;
            // Cria a thread que executa a ação fornecida
            _thread = threadStart;
            _thread.Start();
        }

        // Sinaliza para a thread começar a trabalhar
        public void SignalStart()
        {
            _waitStartEvent.Set();
        }

        // Aguarda a thread terminar (timeout em ms)
        public bool WaitThreadFinish(int timeout = Timeout.Infinite)
        {
            return _thread.Join(timeout);
        }

        // Cancela e tenta finalizar a thread
        public void ExitThread()
        {
            if (!_cts.IsCancellationRequested)
            {
                _cts.Cancel();
            }

            // Sinaliza para o evento caso a thread esteja esperando
            _waitStartEvent.Set();

            // Aguarda a thread terminar (timeout razoável, aqui 5s)
            if (!_thread.Join(5000))
            {
                // Se não terminar, você pode forçar ou logar
                Console.WriteLine("Thread não finalizou no tempo esperado.");
            }
        }

        // A thread pode aguardar pelo evento de start assim:
        public void WaitForStart()
        {
            _waitStartEvent.Wait(_cts.Token);
            // após liberar, limpa o evento para próximo ciclo (se quiser reutilizar)
            _waitStartEvent.Reset();
        }

        public void Dispose()
        {
            ExitThread();
            _waitStartEvent.Dispose();
            _cts.Dispose();
        }
    }
    public class PangyaTimer
    {
        public enum STATE_TIME
        {
            NONE,
            START,
            PAUSE,
            RESUME,
            STOP,
            DELETE,
            INIT,
            RUN,
            FINISH
        }
        private uint tempoBase; // Tempo base em ms
        private DateTime? inicio;
        private TimeSpan acumulado;
        private bool pausado;
        private STATE_TIME state_time = STATE_TIME.NONE;
        private System.Threading.Timer timerLog;

        public PangyaTimer(uint tempoBase) //deve perder 5 segundos
        {
            this.tempoBase = tempoBase;
            this.inicio = null;
            this.acumulado = TimeSpan.Zero;
            this.pausado = false;
            state_time = STATE_TIME.INIT;
        }

        public void start()
        {
            inicio = DateTime.Now;
            acumulado = TimeSpan.Zero;
            pausado = false;
            state_time = STATE_TIME.START;
            StartLogTimer();

            Thread.Sleep(5000);
        }

        public void pause()
        {
            if (inicio != null && !pausado)
            {
                acumulado += DateTime.Now - inicio.Value;
                pausado = true;
                inicio = null;
                state_time = STATE_TIME.PAUSE;
                StopLogTimer();
            }
        }

        public void resume()
        {
            if (pausado)
            {
                inicio = DateTime.Now;
                pausado = false;
                state_time = STATE_TIME.RESUME;
                StartLogTimer();
            }
        }

        public void stop(STATE_TIME _TIME = STATE_TIME.STOP)
        {
            inicio = null;
            acumulado = TimeSpan.Zero;
            pausado = false;
            state_time = _TIME;
            StopLogTimer();
        }

        public void reset(uint? novoTempo = null)
        {
            // Se quiser, pode receber um novo tempo.
            if (novoTempo.HasValue)
                this.tempoBase = novoTempo.Value;

            stop();  // já zera tudo.
        }

        public uint GetElapsedMilliseconds()
        {
            TimeSpan elapsed = acumulado;
            if (inicio != null)
            {
                elapsed += DateTime.Now - inicio.Value;
            }
            return (uint)elapsed.TotalMilliseconds;
        }

        public uint GetRemainingMilliseconds()
        {
            uint elapsed = GetElapsedMilliseconds();
            return tempoBase > elapsed ? tempoBase - elapsed : 0;
        }

        // Inicia timer que mostra o log a cada segundo 
        private void StartLogTimer()
        {
            if (state_time == STATE_TIME.START && timerLog == null)
            {    timerLog = new System.Threading.Timer(_ =>
                {
                    try
                    {
                        state_time = STATE_TIME.RUN;
                        uint remaining = GetRemainingMilliseconds();
                        uint totalSeconds = remaining / 1000;

                        uint minutes = totalSeconds / 60;
                        uint seconds = totalSeconds % 60;

                        message_pool.push(new message($"[PangyaTimer::StartLogTimer][Log] {minutes:D2}:{seconds:D2} ", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        // Para automaticamente quando chegar a zero
                        if (remaining == 0)
                        {
                            stop(STATE_TIME.FINISH);
                        }
                    }
                    catch (Exception ex)
                    {
                        message_pool.push(new message($"[PangyaTimer::StartLogTimer][Error] {ex.Message}", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    }
                }, null, 0, 600);
            }
        }

        // Para o timer de log
        private void StopLogTimer()
        {
            state_time = STATE_TIME.START;
            this.tempoBase = 0;
            this.inicio = null;
            this.acumulado = TimeSpan.Zero;
            this.pausado = false;
            if (timerLog != null)
            {
                timerLog.Dispose();
                timerLog = null;
            }
        }

        public STATE_TIME getState()
        {
            return state_time;
        }
    }
}