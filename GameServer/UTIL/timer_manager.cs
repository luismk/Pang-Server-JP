//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Threading;

//namespace Pangya_GameServer.UTIL
//{
//    public class TimerParam
//    {
//        public Job Job { get; }
//        public JobPool JobPool { get; }

//        public TimerParam(Job job, JobPool jobPool)
//        {
//            Job = job;
//            JobPool = jobPool;
//        }
//    }
//    public class timer_manager
//    {
//        private readonly Timer _timer;
//        private readonly Job _job;
//        private bool _executed = false;
//        private readonly Stopwatch _stopwatch = new Stopwatch();

//        public enum TimerType
//        {
//            Normal,
//            Periodic,
//            PeriodicInfinite
//        }

//        public timer_manager(uint waitMs, Job job, TimerType tipo = TimerType.Normal, List<uint> intervals = null)
//        {
//            _job = job;
//            _stopwatch.Start();

//            _timer = new Timer(_ =>
//            {
//                if (!_executed)
//                {
//                    _executed = true;
//                    job.Execute();
//                }
//            }, null, waitMs, Timeout.Infinite);
//        }

//        public timer_manager CreateTimer(uint milliseconds, TimerParam param, List<uint> intervals)
//        {
//            if (param == null || !param.Job.IsValid())
//                return null;

//            return new timer_manager(milliseconds, param.Job, timer_manager.TimerType.Periodic, intervals);
//        }

//        public void Stop()
//        {
//            _timer.Dispose();
//            _stopwatch.Stop();
//        }

//        public long GetElapsed() => _stopwatch.ElapsedMilliseconds;
//        public TimerState GetState() => !_stopwatch.IsRunning ? TimerState.Stopped : TimerState.Running;

//        public enum TimerState
//        {
//            Stopped,
//            Running
//        }
//    }

//    public class Job
//    {
//        private readonly Action _action;

//        public Job(Action action)
//        {
//            _action = action ?? throw new ArgumentNullException(nameof(action));
//        }

//        public void Execute() => _action();
//    }
//    public class JobPool
//    {
//        private readonly BlockingCollection<Job> _queue = new BlockingCollection<Job>();

//        public void StartWorkerThread()
//        {
//            var thread = new Thread(() => {
//                foreach (var job in _queue.GetConsumingEnumerable())
//                {
//                    try
//                    {
//                        job.Execute();
//                    }
//                    catch (Exception ex)
//                    {
//                        Console.WriteLine("[JobPool] Erro ao executar job: " + ex.Message);
//                    }
//                }
//            });

//            thread.IsBackground = true;
//            thread.Start();
//        }

//        public void Push(Job job)
//        {
//            _queue.Add(job);
//        }

//        public void Stop()
//        {
//            _queue.CompleteAdding(); // Finaliza o loop de consumo
//        }
//    }
//}