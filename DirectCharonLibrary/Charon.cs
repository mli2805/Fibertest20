using System;
using System.Collections.Generic;
using System.Linq;

namespace DirectCharonLibrary
{
    public partial class Charon
    {
        public TcpAddress TcpAddress { get; set; }
        public string Serial { get; set; }
        public int OwnPortCount { get; set; }
        public int FullPortCount { get; set; }
        public int StartPortNumber { get; set; }


        public Charon Parent { get; set; }
        public Dictionary<int, Charon> Children { get; set; }

        public string LastErrorMessage { get; set; }
        public string LastAnswer { get; set; }
        public bool IsLastCommandSuccessful { get; set; }

        public Charon(TcpAddress tcpAddress)
        {
            TcpAddress = tcpAddress;
        }

        public bool Initialize()
        {
            StartPortNumber = Parent == null ? 1 : StartPortNumber = Parent.FullPortCount + 1;
            Children = new Dictionary<int, Charon>();

            Serial = GetSerial();
            if (!IsLastCommandSuccessful)
                return false;
            Serial = Serial.Substring(0, Serial.Length - 2); // "\r\n"

            OwnPortCount = GetOwnPortCount();
            FullPortCount = OwnPortCount;
            if (!IsLastCommandSuccessful)
                return false;

            var expendedPorts = GetExtentedPorts();
            if (!IsLastCommandSuccessful)
                return false;
            if (expendedPorts != null)
                foreach (var expendedPort in expendedPorts)
                {
                    var childCharon = new Charon(expendedPort.Value);
                    childCharon.Parent = this;
                    if (!childCharon.Initialize())
                    {
                        LastErrorMessage = $"Child charon {expendedPort.Value} initialization failed";
                        return false;
                    }
                    Children.Add(expendedPort.Key, childCharon);
                    FullPortCount += childCharon.FullPortCount;
                }

            return true;
        }

        public int GetExtendedActivePort()
        {
            var activePort = GetActivePort();
            if (!Children.ContainsKey(activePort))
                return activePort;

            return Children[activePort].GetActivePort() + Children[activePort].StartPortNumber - 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="port"></param>
        /// <returns>-1 if failed or current active port</returns>
        public int SetExtendedActivePort(int port)
        {
            if (port <= OwnPortCount)
                return SetActivePort(port);

            Charon child = Children?.Values.FirstOrDefault(
                    c => c.StartPortNumber <= port && c.StartPortNumber + c.OwnPortCount > port);
            if (child == null)
            {
                LastErrorMessage = "Out of range port number error";
                return -1;
            }


            var masterPort = Children.First(pair => pair.Value == child).Key;
            if (GetActivePort() != masterPort)
            {
                var newMasterPort = SetActivePort(masterPort);
                if (newMasterPort != masterPort)
                    return -1;
            }

            var portOnChild = port - child.StartPortNumber + 1;
            var resultingPort = child.SetActivePort(portOnChild);
            if (portOnChild != resultingPort)
            {
                LastErrorMessage = child.LastErrorMessage;
                IsLastCommandSuccessful = child.IsLastCommandSuccessful;
                return resultingPort;
            }
            return port;
        }

        public bool DetachOtauFromPort(int fromOpticalPort)
        {
            var extPorts = GetExtentedPorts();
            if (!extPorts.ContainsKey(fromOpticalPort))
            {
                LastErrorMessage = "There is no such extended port. Nothing to do.";
                return false;
            }

            extPorts.Remove(fromOpticalPort);
            var content = DictionaryToContent(extPorts);
            SendWriteIniCommand(content);
            return IsLastCommandSuccessful;
        }

        public bool AttachOtauToPort(TcpAddress additionalOtauAddress, int toOpticalPort)
        {
            var extPorts = GetExtentedPorts();
            if (extPorts.ContainsKey(toOpticalPort))
            {
                Console.WriteLine("This is extended port already. Denied.");
                return true;
            }
            extPorts.Add(toOpticalPort, additionalOtauAddress);
            var content = DictionaryToContent(extPorts);
            SendWriteIniCommand(content);
            return IsLastCommandSuccessful;
        }
    }
}