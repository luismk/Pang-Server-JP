using System;        
namespace MessengerServer
{
    public class MessengerServer
    {
        static void Main(string[] args)
        {
			try
			{ 
                sms.ms.getInstance().Start();
                for (; ; )
                {
                    var comando = Console.ReadLine().Split(new char[] { ' ' }, 2);
                     sms.ms.getInstance().CheckCommand(comando.ToString());
                }
            }
			catch (Exception e)
			{

				throw e;
			}
        }
    }
}
