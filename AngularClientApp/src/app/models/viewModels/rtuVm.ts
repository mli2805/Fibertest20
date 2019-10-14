import { MonitoringMode } from '../enums/monitoringMode';
import { RtuMaker } from '../enums/rtuMaker';
import { NetAddress } from '../dtos/netAddress';
import { RtuPartState } from '../enums/rtuPartState';
import { RtuDto } from '../dtos/rtuDto';

export class RtuVm {
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

  constructor(rtuDto: RtuDto) {
    this.rtuId = rtuDto.rtuId;
    this.title = rtuDto.title;
    this.fullPortCount = rtuDto.fullPortCount;
    this.ownPortCount = rtuDto.ownPortCount;
  }
}
