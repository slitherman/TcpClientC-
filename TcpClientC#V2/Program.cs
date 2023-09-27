using System.Net.Sockets;
using System.Text;

namespace TcpClientC_V2
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Chat client is running...");
                TcpClient client = new TcpClient("localhost", 6666);
                NetworkStream stream = client.GetStream();
                Console.WriteLine("Enter your username");
                //sends the username to the server
                string username = Console.ReadLine();
                byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
                await stream.WriteAsync(usernameBytes, 0, usernameBytes.Length);

                //creates a seperate thread for receiving messages from the server
                Task.Run(async () => await ReceiveMessages(stream));


                // send messages or files
                while (true)
                {
                    string input = Console.ReadLine();
                    if (input.StartsWith("/file"))
                    {
                        // handling file upload
                        await SendFile(stream, input);
                    }
                    else
                    {
                        //sending regular chat messages
                        byte[] messageBytes = Encoding.UTF8.GetBytes(input);
                        await stream.WriteAsync(messageBytes, 0, messageBytes.Length);

                    }
                }
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("An error occurred: " + ex.Message);

            }
        }
        private static async Task SendFile(NetworkStream stream, string input)
        {
            try
            {
                string[] parts = input.Split(' ');
                if (parts.Length != 3)
                {
                    Console.WriteLine("Invalid file upload command. Use '/file <file_path> <file_name>'");
                    return;
                }
                string filePath = parts[1];
                string fileName = parts[2];

                byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName);
                byte[] fileData = File.ReadAllBytes(filePath);

                string fileTransferMessage = $"/file|{fileName}|{fileData.Length}";
                byte[] messageBytes = Encoding.UTF8.GetBytes(fileTransferMessage);

                //sending the file message transfer to the server
                await stream.WriteAsync(messageBytes, 0, messageBytes.Length);

                // send the file data to the server
                await stream.WriteAsync(fileData, 0, fileData.Length);


            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("An error occurred while sending the file: " + ex.Message);

            }
        }

        private static async Task ReceiveMessages(NetworkStream stream)
        {
            try
            {
                while (true)
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        Console.WriteLine("Disconnected from the server.");
                        break;

                    }
                    //server response
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("Received: " + message);

                }
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("An error occurred while receiving messages: " + ex.Message);
            }
        }
    }
}