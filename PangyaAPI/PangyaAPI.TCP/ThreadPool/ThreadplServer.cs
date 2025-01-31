using PangyaAPI.TCP.Session;    
using PangyaAPI.TCP.Util;
using System;
namespace PangyaAPI.TCP.ThreadPool
{
    public class ThreadplServer : MyThreadPool
    {
        protected int numThreadWorkersIO;
        protected int numThreadWorkersLogical;
        protected uint jobThreadNum;
        public enum OperationType : int
        {
            SendRawCompleted = 1,
            SendCompleted = 2, RecvCompleted = 3
        }
        public ThreadplServer(int numThreadWorkersIO, int numThreadWorkersLogical, uint jobThreadNum) : base(numThreadWorkersIO, numThreadWorkersLogical, jobThreadNum)
        {
            this.numThreadWorkersIO = numThreadWorkersIO;
            this.numThreadWorkersLogical = numThreadWorkersLogical;
            this.jobThreadNum = jobThreadNum;
        }

      
        public void TranslatePacket(SessionBase session, PangyaBuffer lpBuffer, int dwIOsize, int operation)
        {
            switch ((OperationType)operation)
            {
                case OperationType.SendRawCompleted:
                    HandleSendRawCompleted(session, lpBuffer, dwIOsize);
                    break;

                case OperationType.SendCompleted:
                    HandleSendCompleted(session, lpBuffer, dwIOsize);
                    break;

                case OperationType.RecvCompleted:
                    HandleRecvCompleted(session, lpBuffer, dwIOsize);
                    break;

                default:
                    break;
            }
        }

        private void HandleSendRawCompleted(SessionBase session, PangyaBuffer lpBuffer, int dwIOsize)
        {
            if (dwIOsize > 0 && lpBuffer != null)
            {
                try
                {
                    lpBuffer.Consume(dwIOsize);  
                }
                catch (Exception e)
                {
                    ClearPacketLoop(session);     
                    LogError($"ErrorSystem: {e.Message}");
                }
            }
            else
            {                            
                try
                {
                    if (session.GetConnectTime() <= 0 && session.GetState())
                    {
                        LogError($"[Error] SessionBaseBase[OID={session.m_oid}] is not connected.");
                    }
                }
                catch (Exception e)
                {
                    LogError($"ErrorSystem: {e.Message}");
                }
            }
        }

        private void HandleSendCompleted(SessionBase session, PangyaBuffer lpBuffer, int dwIOsize)
        {
            if (dwIOsize > 0 && lpBuffer != null)
            {
                try
                {
                    // Here you should have some logic to process the packet in lpBuffer.
                    // For simplicity, this is just a placeholder.
                    // Example of calling a method to process packets:
                    ProcessPacket(session, lpBuffer);
                }
                catch (Exception e)
                {
                    LogError($"SEND_COMPLETED Exception: {e.Message}");
                    ClearPacketLoop(session);       
                    return;
                }
            }
            else
            {                             
                try
                {
                    if (session.GetConnectTime() <= 0 && session.GetState())
                    {
                        LogError($"[Error] SessionBaseBase[OID={session.m_oid}] is not connected.");
                    }
                }
                catch (Exception e)
                {
                    LogError($"ErrorSystem: {e.Message}");
                }
            }
        }

        private void HandleRecvCompleted(SessionBase session, PangyaBuffer lpBuffer, int dwIOsize)
        {
            if (dwIOsize > 0 && lpBuffer != null)
            {
                try
                {
                    lpBuffer.AddSize(dwIOsize);

                    // Example logic for handling received data
                    ProcessReceivedData(session, lpBuffer);
                }
                catch (Exception e)
                {
                    LogError($"RECV_COMPLETED Exception: {e.Message}");  
                    return;
                }
            }
            else
            {
                DisconnectSessionBase(session);
            }
        }

        private void ProcessPacket(SessionBase session, PangyaBuffer buffer)
        {
            // Custom processing for sending packets
            // You should implement your logic here
        }

        private void ProcessReceivedData(SessionBase session, PangyaBuffer buffer)
        {
            // Custom processing for received data
            // You should implement your logic here
        }

        private void DisconnectSessionBase(SessionBase session)
        {
            // Disconnect logic for session
            //session.Disconnect();
        }

        private void ClearPacketLoop(SessionBase session)
        {
            // Logic to clear any active packet processing
        }

        private void LogError(string message)
        {
            // Log the error to console or a file
            Console.WriteLine(message);
        }
    }
}
