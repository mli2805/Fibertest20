import { NetAddress } from "../../underlying/netAddress";

export class AttachOtauDto {
  ClientIp: string;

  OtauId: string;
  RtuId: string;

  NetAddress: NetAddress;
  OpticalPort: number;
}
