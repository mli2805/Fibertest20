import { RtuMaker } from "../../enums/rtuMaker";
import { NetAddress } from "../../underlying/netAddress";

export class DetachOtauDto {
  otauId: string;
  rtuId: string;
  rtuMaker: RtuMaker;
  netAddress: NetAddress;
  opticalPort: number;
}
