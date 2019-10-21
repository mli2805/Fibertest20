import { BaseRefInfoDto } from './baseRefInfoDto';
import { MeasurementDto } from './measurementDto';

export class TraceStatisticsDto {
  traceTitle: string;
  port: string; // for trace on bop use bop's serial plus port number "879151-3"
  rtuTitle: string;
  baseRefs: BaseRefInfoDto[];
  measurements: MeasurementDto[];
}
