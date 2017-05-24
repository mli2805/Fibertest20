using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Iit.Fibertest.DirectCharonLibrary
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
                var client = new TcpClient();
                var connection = client.BeginConnect(NetAddress.Ip4Address, NetAddress.Port, null, null);
                var success = connection.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(4));
                if (!success)
                {
                    LastErrorMessage = "Can't establish connection. Check connection timeout";
                    if (_charonLogLevel >= CharonLogLevel.TransmissionCommands)
                        _rtuLogger35.AppendLine(LastErrorMessage);
                    return;
                }
                client.SendTimeout = TimeSpan.FromSeconds(2).Milliseconds;
                client.ReceiveTimeout = TimeSpan.FromSeconds(2).Milliseconds;

                NetworkStream nwStream = client.GetStream();
                byte[] bytesToSend = Encoding.ASCII.GetBytes(cmd);

                //---send the text---
                if (_charonLogLevel >= CharonLogLevel.TransmissionCommands)
                    _rtuLogger35.AppendLine(cmd, 4, "Sending :");
                nwStream.Write(bytesToSend, 0, bytesToSend.Length);

                // for bulk command could be needed
                Thread.Sleep(200);

                //---read back the text---
                byte[] bytesToRead = new byte[client.ReceiveBufferSize];
                int bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
                client.Close();
                if (_charonLogLevel >= CharonLogLevel.TransmissionCommands)
                    _rtuLogger35.AppendLine(Encoding.ASCII.GetString(bytesToRead, 0, bytesRead), 4, "Received :");
                LastAnswer = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                IsLastCommandSuccessful = true;
            }
            catch (Exception e)
            {
                if (_charonLogLevel >= CharonLogLevel.TransmissionCommands)
                    _rtuLogger35.AppendLine(e.Message);
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
                var client = new TcpClient();
                var connection = client.BeginConnect(NetAddress.Ip4Address, NetAddress.Port, null, null);
                var success = connection.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(4));
                if (!success)
                {
                    LastErrorMessage = "Can't establish connection. Check connection timeout";
                    if (_charonLogLevel >= CharonLogLevel.TransmissionCommands)
                        _rtuLogger35.AppendLine(LastErrorMessage);
                    return;
                }
                client.SendTimeout = TimeSpan.FromSeconds(2).Milliseconds;
                client.ReceiveTimeout = TimeSpan.FromSeconds(4).Milliseconds;

                NetworkStream nwStream = client.GetStream();

                //---send the command---
                byte[] bytesToSend = Encoding.ASCII.GetBytes(cmd);
                if (_charonLogLevel >= CharonLogLevel.TransmissionCommands)
                    _rtuLogger35.AppendLine(cmd, 4, "Sending :");
                nwStream.Write(bytesToSend, 0, bytesToSend.Length);

                //---read back the answer---
                byte[] bytesToRead = new byte[client.ReceiveBufferSize];
                int bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
                if (_charonLogLevel >= CharonLogLevel.TransmissionCommands)
                    _rtuLogger35.AppendLine(Encoding.ASCII.GetString(bytesToRead, 0, bytesRead), 4, "Received :");

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
                if (_charonLogLevel >= CharonLogLevel.TransmissionCommands)
                    _rtuLogger35.AppendLine(Encoding.ASCII.GetString(bytesToRead, 0, bytesRead), 4, "Received : ");

                client.Close();
                LastAnswer = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                IsLastCommandSuccessful = true;
            }
            catch (Exception e)
            {
                if (_charonLogLevel >= CharonLogLevel.TransmissionCommands)
                    _rtuLogger35.AppendLine(e.Message);
                LastErrorMessage = e.Message;
            }

        }

    }

}
