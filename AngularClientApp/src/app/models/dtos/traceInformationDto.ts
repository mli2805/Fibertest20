import { TraceInfoTableItem } from '../underlying/traceInfoTableItem';

export class TraceInformationDto {
  traceTitle: string;
  port: string; // for trace on bop use bop's serial plus port number "879151-3"
  rtuTitle: string;

  equipment: TraceInfoTableItem[];
  nodes: TraceInfoTableItem[];

  isLightMonitoring: boolean;
  comment: string;
}
