import { NetAddress } from "../../underlying/netAddress";
import { RtuMaker } from "../../enums/rtuMaker";

export class AttachOtauDto {
  ClientIp: string;

  OtauId: string;
  RtuId: string;
  RtuMaker: RtuMaker;

  OtauAddress: NetAddress;
  OpticalPort: number;
}
