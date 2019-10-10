import { MonitoringMode } from '../enums/monitoringMode';
import { RtuMaker } from '../enums/rtuMaker';
import { NetAddress } from './netAddress';
import { RtuPartState } from '../enums/rtuPartState';

export interface RtuDto {
  rtuId: string;
  rtuMaker: RtuMaker;
  title: string;

  fullPortCount: number;
  ownPortCount: number;

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
