using System;
using Dto;

namespace Iit.Fibertest.UtilsLib
{
    public class RtuStation
    {
        public Guid Id { get; set; }
        public string Version { get; set; }

        public DoubleAddressWithLastConnectionCheck PcAddresses { get; set; } = new DoubleAddressWithLastConnectionCheck();

        /// <summary>
        /// Charon(Otdr,Otau) always has only one address, no matter how many addresses has PC
        /// This address should be set up by user in RTU ini file
        /// 
        /// For MAK100 it is always 192.168.88.101 as a sign that RTU is MAK100
        ///     and you should use RTU address (main or reserve whatever is available) to connect Otdr
        /// For old types of RTU it is usually +1 to main address of PC 
        ///     (but could be any user wants)
        /// 
        /// The only reason to store this address is to pass it to Reflect
        /// for ReflectMeasure mode, so Reflect could connect Otdr directly
        /// </summary>
        public string OtdrIp { get; set; }

        public override string ToString()
        {
            return Id.First6();
        }
    }
}