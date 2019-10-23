export class TraceInformationDto {
  traceTitle: string;
  port: string; // for trace on bop use bop's serial plus port number "879151-3"
  rtuTitle: string;

  equipment: object[];
  isLightMonitoring: boolean;
  comment: string;
}
