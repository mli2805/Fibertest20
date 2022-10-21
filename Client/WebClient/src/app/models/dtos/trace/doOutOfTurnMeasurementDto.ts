import { PortWithTraceDto } from "../../underlying/portWithTraceDto";

export class DoOutOfTurnMeasurementDto {
  connectionId: string;
  rtuId: string;
  portWithTraceDto: PortWithTraceDto;
}
