import { ReturnCode } from "../../enums/returnCode";

export class ClientMeasurementDoneDto {
  connectionId: string;
  clientIp: string;
  returnCode: ReturnCode;
  sorBytes: number[];
  id: string;
}
