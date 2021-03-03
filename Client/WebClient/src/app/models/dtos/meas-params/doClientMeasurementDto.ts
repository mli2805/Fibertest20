import { OtauPortDto } from "../../underlying/otauPortDto";

export class DoClientMeasurementDto {
  connectionId: string;
  clientIp: string;
  rtuId: string;

  selectedMeasParams: object;

  otauPortDto: OtauPortDto;
  otauIp: string;
  otauTcpPort: number;
}
