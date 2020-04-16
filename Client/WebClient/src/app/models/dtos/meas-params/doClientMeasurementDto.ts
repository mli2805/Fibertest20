import { OtauPortDto } from "../../underlying/otauPortDto";

export class DoClientMeasurementDto {
  clientIp: string;
  rtuId: string;

  selectedMeasParams: object;

  otauPortDto: OtauPortDto;
  otauIp: string;
  otauTcpPort: number;
}
