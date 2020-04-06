import { NetAddress } from "../../underlying/netAddress";

export class DetachOtauDto {
  otauId: string;
  rtuId: string;
  otauAddress: NetAddress;
  opticalPort: number;
}
