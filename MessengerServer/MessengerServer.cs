using System;        
namespace MessengerServer
{
    public class MessengerServer
    {
        static void Main(string[] args)
        {
            sms.ms.getInstance().Start();
            for (; ; )
            {
                var comando = Console.ReadLine().Split(new char[] { ' ' }, 2);
                sms.ms.getInstance().RunCommand(comando);
            }
        }
    }
}
