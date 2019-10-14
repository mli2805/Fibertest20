import { ChildDto } from './childDto';
import { NetAddress } from './netAddress';

export class OtauWebDto extends ChildDto{
  otauId: string;
  rtuId: string;
  otauNetAddress: NetAddress;
  isOk: boolean;

  children: ChildDto[];
}
