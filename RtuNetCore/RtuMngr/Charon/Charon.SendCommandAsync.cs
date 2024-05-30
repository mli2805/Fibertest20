using System.Net.Sockets;
using System.Text;
using Iit.Fibertest.UtilsNetCore;

namespace Iit.Fibertest.RtuMngr
{
    public partial class Charon
    {
        private async Task SendCommandAsync(string cmd)
        {
            await Task.Delay(TimeSpan.FromMicroseconds(_pauseBetweenCommands));
            LastAnswer = "";
            LastErrorMessage = "";
            IsLastCommandSuccessful = false;

            try
            {
                _logger.Debug(Logs.RtuManager, $"    SendCommand <<{cmd.Trim()}>> to {NetAddress.ToStringA()}");
                var client = new TcpClient();
                client.SendTimeout = _writeTimeout * 1000;
                client.ReceiveTimeout = _readTimeout * 1000;

                await client.ConnectAsync(NetAddress.Ip4Address, NetAddress.Port).WaitAsync(TimeSpan.FromSeconds(_connectionTimeout));
                if (client.Connected)
                {
                    // _logger.Debug(Logs.RtuManager, "    connected successfully");
                }
                else
                {
                    LastErrorMessage = "Can't establish connection. Check connection timeout";
                    _logger.Error(Logs.RtuManager, LastErrorMessage);
                    return;
                }

                NetworkStream nwStream = client.GetStream();
                byte[] bytesToSend = Encoding.ASCII.GetBytes(cmd);

                //---send the text---
                nwStream.Write(bytesToSend, 0, bytesToSend.Length);

                // for bulk command could be needed
                await Task.Delay(TimeSpan.FromMilliseconds(200));

                //---read back the text---
                byte[] bytesToRead = new byte[client.ReceiveBufferSize];
                int bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
                //_logger.Debug(Logs.RtuManager, $"stream received {bytesRead} bytes");

                client.Close();
                LastAnswer = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                _logger.Debug(Logs.RtuManager, $"    Received : {LastAnswer.Trim()}");
                IsLastCommandSuccessful = true;
            }
            catch (Exception e)
            {
                _logger.Exception(Logs.RtuManager, e, $"SendCommand: {cmd.Trim()}");
                IsLastCommandSuccessful = false;
                LastErrorMessage = e.Message;
            }
        }

        private async Task SendWriteIniCommandAsync(string content)
        {
            await Task.Delay(_pauseBetweenCommands);
            LastAnswer = "";
            LastErrorMessage = "";
            IsLastCommandSuccessful = false;
            string cmd = "ini_write\r\n";

            try
            {
                var client = new TcpClient();
                await client.ConnectAsync(NetAddress.Ip4Address, NetAddress.Port).WaitAsync(TimeSpan.FromSeconds(_connectionTimeout));
                if (client.Connected)
                {
                    _logger.Debug(Logs.RtuManager, "    connected successfully");
                }
                else
                {
                    LastErrorMessage = "Can't establish connection. Check connection timeout";
                    _logger.Error(Logs.RtuManager, LastErrorMessage);
                    return;
                }

                client.SendTimeout = _writeTimeout * 1000;
                client.ReceiveTimeout = _readTimeout * 1000;

                NetworkStream nwStream = client.GetStream();
                //---send the command---
                byte[] bytesToSend = Encoding.ASCII.GetBytes(cmd);
                _logger.Debug(Logs.RtuManager, $"Sending : {cmd.Trim()}");
                nwStream.Write(bytesToSend, 0, bytesToSend.Length);

                //---read back the answer---
                byte[] bytesToRead = new byte[client.ReceiveBufferSize];
                int bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
                LastAnswer = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                _logger.Debug(Logs.RtuManager, $"Received : {LastAnswer.Trim()}");

                //---send the content---
                byte[] contentBytes = new byte[CharonIniSize];
                var contentB = Encoding.ASCII.GetBytes(content);
                Array.Copy(contentB, contentBytes, contentB.Length);

                int rest = CharonIniSize;
                while (rest > 0)
                {
                    if (rest >= 256)
                    {
                        byte[] bytes256 = new byte[256];
                        Array.Copy(contentBytes, CharonIniSize - rest, bytes256, 0, 256);
                        nwStream.Write(bytes256, 0, bytes256.Length);
                        rest = rest - 256;
                    }
                    else
                    {
                        byte[] bytes = new byte[rest];
                        Array.Copy(contentBytes, CharonIniSize - rest, bytes, 0, rest);
                        nwStream.Write(bytes, 0, bytes.Length);
                        rest = 0;
                    }
                }

                await Task.Delay(TimeSpan.FromMilliseconds(1000));

                //---read back the answer---
                bytesToRead = new byte[client.ReceiveBufferSize];
                bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
                LastAnswer = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                _logger.Debug(Logs.RtuManager, $"Received : {LastAnswer.Trim()}");

                client.Close();
                IsLastCommandSuccessful = true;
            }
            catch (Exception e)
            {
                _logger.Exception(Logs.RtuManager, e, "SendWriteIniCommand");
                LastErrorMessage = e.Message;
            }
        }
    }
}
