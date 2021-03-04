import { OtauPortDto } from "../../underlying/otauPortDto";

export class DoClientMeasurementDto {
  connectionId: string;
  clientIp: string;
  rtuId: string;

  selectedMeasParams: MeasParam[];

  otauPortDto: OtauPortDto;
  otauIp: string;
  otauTcpPort: number;
}

export class MeasParam {
  param: number;
  value: number;

  constructor(param: number, value: number) {
    this.param = param;
    this.value = value;
  }
}
