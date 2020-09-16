import { NetAddress } from "../../underlying/netAddress";

export class DetachOtauDto {
  otauId: string;
  rtuId: string;
  netAddress: NetAddress;
  opticalPort: number;
}
