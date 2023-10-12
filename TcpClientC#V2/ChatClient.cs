using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TcpClientC_V2
{
    public class ChatClient
    {
        private NetworkStream stream;
        private TcpClient client;


        public ChatClient()
        {
            client = new TcpClient("localhost", 6666);
            stream = client.GetStream();

        }
        public async Task Start()
        {
            Console.WriteLine("Chat client is running...");
            Console.WriteLine("Enter your username");
            string username = Console.ReadLine();
            byte[] userNameBytes = Encoding.UTF8.GetBytes(username);  
            await stream.WriteAsync(userNameBytes,0,username.Length);

            // Start a separate thread for receiving messages from the server
            Task.Run(async () => await ReceiveMessages());

            //main chat loop
            while (true)
            {
                Console.Write("Enter your message (or '/file <file_path> <file_name>' to send a file): ");
                string input = Console.ReadLine();
                if (input.StartsWith("/file"))
                {
                    Console.WriteLine("Sending File...");
                    // handling file upload
                    await SendFile(stream,input);
                }
                else
                {
                    //sending regular chat messages
                    byte[] messageBytes = Encoding.UTF8.GetBytes(input);
                    await stream.WriteAsync(messageBytes,0,messageBytes.Length); 
                    
                }

            }

        }

        private async Task SendFile(NetworkStream stream, string input)
        {
            try
            {
                // Split the input into parts
                string[] parts = input.Split(' ');

                // Check if there are three parts (command, file path, and file name)
                if (parts.Length != 3 || parts[0] != "/file")
                {
                    Console.WriteLine("Invalid file upload command. Use '/file <file_path> <file_name>'");
                    return;
                }

                string filePath = parts[1];
                string fileName = parts[2];
               if(File.Exists(filePath)) {

                    byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName);
                    byte[] fileData = File.ReadAllBytes(filePath);
                   

                    string fileTransferMessage = $"/file:{fileName}:{fileData.Length}";
                    byte[] messageBytes = Encoding.UTF8.GetBytes(fileTransferMessage);

          
                    // Sending the file message transfer to the server
                    await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
                    //Sending the file name
                    await stream.WriteAsync(fileNameBytes, 0, fileNameBytes.Length);
                    // Sending the file data to the server
                    await stream.WriteAsync(fileData, 0, fileData.Length);
                    Console.WriteLine($"Sent file: {fileName} ({fileData.Length} bytes)");

                }
               else
                {
                    Console.WriteLine($"File not found: {filePath}");
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while sending the file: " + ex.Message);
            }
        }


        private async Task ReceiveMessages()
        {
            try
            {

                while (true)
                {

                    byte[] buffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        // Connection closed by the server.
                        Console.WriteLine("Disconnected from the server.");
                        break;
                    }
                    //server response
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    if (message.StartsWith("/file:"))
                    {
                        // This is a file transfer message
                        string[] parts = message.Split(' ');
                        if(parts.Length == 3) 
                        {
                            string fileName = parts[1];
                            int fileSize = int.Parse(parts[2]);
                            // Read the file content.
                            byte[] fileContent = new byte[fileSize];    
                            int totalBytesRead = 0;
                            while (totalBytesRead < fileSize) 
                            {
                                int bytesReceived = await stream.ReadAsync(fileContent, totalBytesRead, fileSize - totalBytesRead);
                                if (bytesReceived == 0)
                                {
                                    break;
                                }
                                totalBytesRead += bytesReceived;
                            }
                            File.WriteAllBytesAsync(fileName, fileContent);
                            Console.WriteLine($"Received file: {fileName}");

                        }
                    }
                    else
                    {
                        Console.WriteLine($" {message}");
                    }
                     
                    
                }
            }
            catch (Exception ex) 
            {
                await Console.Out.WriteLineAsync("An error occurred while receiving messages:" + ex.Message);
            }
        }
    }
}
