import { ChildDto } from './childDto';
import { NetAddress } from './netAddress';

export interface OtauWebDto {
  otauId: string;
  rtuId: string;
  otauNetAddress: NetAddress;
  isOk: boolean;

  children: ChildDto[];
}
