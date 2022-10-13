import { RtuMaker } from "../../enums/rtuMaker";
import { NetAddress } from "../../underlying/netAddress";

export class DetachOtauDto {
  connectionId: string;
  otauId: string;
  rtuId: string;
  rtuMaker: RtuMaker;
  netAddress: NetAddress;
  opticalPort: number;
}
