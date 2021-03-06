export class RtuNetworkSettingsDto {
  rtuTitle: string;

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
