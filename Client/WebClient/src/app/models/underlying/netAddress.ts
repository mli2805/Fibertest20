export class NetAddress {
  Ip4Address: string;
  HostName: string;
  Port: number;
  IsAddressSetAsIp: boolean;

  get toStringASpace(): string {
    return this.Ip4Address + " : " + this.Port;
  }
}
