import { MonitoringMode } from '../enums/monitoringMode';
import { RtuMaker } from '../enums/rtuMaker';
import { NetAddress } from './netAddress';
import { RtuPartState } from '../enums/rtuPartState';
import { ChildDto } from './childDto';

export interface RtuDto {
  rtuId: string;
  rtuMaker: RtuMaker;
  title: string;

  mfid: string;
  mfsn: string;
  omid: string;
  omsn: string;

  fullPortCount: number;
  ownPortCount: number;

  children: ChildDto[];

  mainChannel: NetAddress;
  mainChannelState: RtuPartState;
  reserveChannel: NetAddress;
  reserveChannelState: RtuPartState;
  isReserveChannelSet: boolean;
  otdrNetAddress: NetAddress;
  bopState: RtuPartState;

  monitoringMode: MonitoringMode;

  version: string;
  version2: string;
}
