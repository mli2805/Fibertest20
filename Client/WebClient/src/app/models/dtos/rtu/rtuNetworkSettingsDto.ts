import { RtuMaker } from "../../enums/rtuMaker";

export class RtuNetworkSettingsDto {
  rtuTitle: string;
  rtuMaker: RtuMaker;

  mainChannel: string;
  isReserveChannelSet: boolean;
  reserveChannel: string;
  otdrAddress: string;

  mfid: string;
  serial: string;
  ownPortCount: number;
  fullPortCount: number;
  version: string;
  version2: string;
}
