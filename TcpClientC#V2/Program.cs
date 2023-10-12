using System.Net.Sockets;
using System.Text;

namespace TcpClientC_V2
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
           ChatClient chatClient = new ChatClient();
           await chatClient.Start();
        }

       }
    }
