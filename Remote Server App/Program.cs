using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Security.Cryptography.X509Certificates;

namespace Remote_Server_App
{
    public class Program
    {
        static async Task Main()
        {
            ServerLogic serverLogic = new ServerLogic();
            await serverLogic.ServerRun();
        }
        
    }
}