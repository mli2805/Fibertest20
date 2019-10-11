import { NetAddress } from './netAddress';

export interface OtauDto {
  
  serial: string;
  netAddress: NetAddress;
  ownPortCount: number;
  isOk: boolean;
}
