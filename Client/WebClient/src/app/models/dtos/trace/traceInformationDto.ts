import { TraceInfoTableItem } from "../../underlying/traceInfoTableItem";
import { TraceHeaderDto } from "./traceHeaderDto";

export class TraceInformationDto {
  header: TraceHeaderDto = new TraceHeaderDto();

  equipment: TraceInfoTableItem[];
  nodes: TraceInfoTableItem[];

  isLightMonitoring: boolean;
  comment: string;
}
