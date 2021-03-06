﻿using System;
using System.ServiceModel;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.SuperClient
{
    public sealed class SuperClientWcfServiceHost
    {
        private ServiceHost _wcfHost;
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly SuperClientWcfService _superClientWcfService;

        public SuperClientWcfServiceHost(IniFile iniFile, IMyLog logFile, SuperClientWcfService superClientWcfService)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _superClientWcfService = superClientWcfService;
        }

        public void StartWcfListener()
        {
            try
            {
                var uri = new Uri(WcfFactory.CombineUriString(@"localhost",
                    (int)TcpPorts.SuperClientListenTo, @"WcfServiceInSuperClient"));
                _wcfHost = new ServiceHost(_superClientWcfService);
                _wcfHost.AddServiceEndpoint(typeof(IWcfServiceInSuperClient),
                    WcfFactory.CreateDefaultNetTcpBinding(_iniFile), uri);
                _wcfHost.Open();
                _logFile.AppendLine(@"Children clients listener started successfully");
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                throw;
            }
        }
    }
}
