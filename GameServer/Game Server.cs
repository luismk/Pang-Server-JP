using System; 
namespace GameServer
{
    public class GameServer
    {
        static void Main(string[] args)
        {    
            sgs.gs.getInstance().Start();
            for (; ; )
            {
                var comando = Console.ReadLine().Split(new char[] { ' ' }, 2);
                sgs.gs.getInstance().RunCommand(comando);
            }
        }
    }
}
