using System.Collections.Concurrent;
using System.Threading;
using System;
using PangyaAPI.TCP.Util;
using PangyaAPI.TCP.Session;

namespace PangyaAPI.TCP.ThreadPool
{
    public class MyThreadPool
    {
        private readonly BlockingCollection<Action> _workQueue;
        private readonly Thread[] _workers;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public MyThreadPool(int numThreadWorkersIO, int numThreadWorkersLogical, uint jobThreadNum)
        {
            _workQueue = new BlockingCollection<Action>(new ConcurrentQueue<Action>());
            _cancellationTokenSource = new CancellationTokenSource();
            _workers = new Thread[numThreadWorkersIO + numThreadWorkersLogical];

            // Inicializa os trabalhadores de threads
            for (int i = 0; i < _workers.Length; i++)
            {
                _workers[i] = new Thread(WorkLoop);
                _workers[i].Start();
            }
        }

        private void WorkLoop()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var workItem = _workQueue.Take(_cancellationTokenSource.Token); // Bloqueia até que um trabalho esteja disponível
                    workItem(); // Executa a tarefa
                }
                catch (OperationCanceledException)
                {
                    // Tolerar cancelamento
                }
            }
        }

        public void Execute(Action workItem)
        {
            if (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                _workQueue.Add(workItem); // Enfileira a tarefa para execução
            }
            else
            {
                throw new InvalidOperationException("Thread pool is shutting down.");
            }
        }

        public void ThreadStop()
        {
            _cancellationTokenSource.Cancel();
            foreach (var worker in _workers)
            {
                worker.Join(); // Espera as threads terminarem
            }
        }
    }

    public class ThreadPoolMessage
    {
        public SessionBase Session { get; set; }
        public PangyaBuffer Buffer { get; set; }
        public uint Operation { get; set; }
    }

}