using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DirectCharonLibrary
{
    public partial class Charon
    {
        private void SendCommand(string cmd)
        {
            LastAnswer = "";
            LastErrorMessage = "";
            IsLastCommandSuccessful = false;
            try
            {
                //---create a TCPClient object at the IP and port no.---
                TcpClient client = new TcpClient(TcpAddress.Ip, TcpAddress.TcpPort);
                NetworkStream nwStream = client.GetStream();
                byte[] bytesToSend = Encoding.ASCII.GetBytes(cmd);

                //---send the text---
                // Console.WriteLine("Sending : \n" + cmd);
                nwStream.Write(bytesToSend, 0, bytesToSend.Length);

                // for bulk command could be needed
                Thread.Sleep(200);

                //---read back the text---
                byte[] bytesToRead = new byte[client.ReceiveBufferSize];
                int bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
                client.Close();
                // Console.WriteLine("Received : \n" + Encoding.ASCII.GetString(bytesToRead, 0, bytesRead));
                LastAnswer = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                IsLastCommandSuccessful = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                LastErrorMessage = e.Message;
            }
        }

        private void SendWriteIniCommand(string content)
        {
            LastAnswer = "";
            LastErrorMessage = "";
            IsLastCommandSuccessful = false;
            string cmd = "ini_write\r\n";
            try
            {
                //---create a TCPClient object at the IP and port no.---
                TcpClient client = new TcpClient(TcpAddress.Ip, TcpAddress.TcpPort);
                NetworkStream nwStream = client.GetStream();

                //---send the command---
                byte[] bytesToSend = Encoding.ASCII.GetBytes(cmd);
                // Console.WriteLine("Sending : \n" + cmd);
                nwStream.Write(bytesToSend, 0, bytesToSend.Length);

                Thread.Sleep(100);

                //---read back the answer---
                byte[] bytesToRead = new byte[client.ReceiveBufferSize];
                int bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
                LastAnswer = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                // Console.WriteLine("Received : \n" + Encoding.ASCII.GetString(bytesToRead, 0, bytesRead));

                //---send the content---
                byte[] contentBytes = new byte[480];
                var contentB = Encoding.ASCII.GetBytes(content);
                Array.Copy(contentB, contentBytes, contentB.Length);

                byte[] bytes256 = new byte[256];
                Array.Copy(contentBytes, bytes256, 256);
                nwStream.Write(bytes256, 0, bytes256.Length);

                byte[] bytes224 = new byte[224];
                Array.Copy(contentBytes, 256, bytes224, 0, 224);
                nwStream.Write(bytes224, 0, bytes224.Length);

                Thread.Sleep(1000);

                //---read back the answer---
                bytesToRead = new byte[client.ReceiveBufferSize];
                bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
                // Console.WriteLine("Received : \n" + Encoding.ASCII.GetString(bytesToRead, 0, bytesRead));

                client.Close();
                LastAnswer = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                IsLastCommandSuccessful = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                LastErrorMessage = e.Message;
            }

        }

    }

}
