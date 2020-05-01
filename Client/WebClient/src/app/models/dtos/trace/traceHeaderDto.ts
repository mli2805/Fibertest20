export class TraceHeaderDto {
  traceTitle: string;
  // for detached trace "-1";
  // for trace on bop use bop's serial plus port number "879151-3"
  port: string;
  rtuTitle: string;
  rtuVersion: string;
}
