using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllClearGameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            TCPGServer server = new TCPGServer(80);

            while (server.MainProcess())
            {

            }
            Console.WriteLine("서버 종료");
            server.ReleaseServer();
        }
    }
}
