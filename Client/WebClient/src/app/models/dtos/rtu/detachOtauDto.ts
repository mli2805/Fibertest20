import { NetAddress } from "../../underlying/netAddress";

export class DetachOtauDto {
  otauId: string;
  rtuId: string;
  otauAddresses: NetAddress;
  opticalPort: number;
}
