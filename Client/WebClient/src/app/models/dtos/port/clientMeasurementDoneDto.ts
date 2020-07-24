import { ReturnCode } from "../../enums/returnCode";

export class ClientMeasurementDoneDto {
  clientIp: string;
  returnCode: ReturnCode;
  sorBytes: number[];
  id: string;
}
