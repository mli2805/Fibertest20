import { NetAddress } from "../../underlying/netAddress";
import { RtuMaker } from "../../enums/rtuMaker";

export class AttachOtauDto {
  ConnectionId: string;
  ClientIp: string;

  OtauId: string;
  RtuId: string;
  RtuMaker: RtuMaker;

  NetAddress: NetAddress;
  OpticalPort: number;
}
