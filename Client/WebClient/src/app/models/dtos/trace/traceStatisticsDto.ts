import { BaseRefInfoDto } from "../../underlying/baseRefInfoDto";
import { MeasurementDto } from "../measurementDto";
import { TraceHeaderDto } from "./traceHeaderDto";

export class TraceStatisticsDto {
  header: TraceHeaderDto;
  baseRefs: BaseRefInfoDto[];
  measFullCount: number;
  measPortion: MeasurementDto[];
}
