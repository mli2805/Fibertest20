import { ChildDto } from "./childDto";
import { NetAddress } from "../../underlying/netAddress";

export class OtauWebDto extends ChildDto {
  expanded: boolean;

  otauId: string;
  rtuId: string;

  otauNetAddress: NetAddress;
  serial: string;

  masterPort: number;
  isOk: boolean;

  children: ChildDto[];
}
