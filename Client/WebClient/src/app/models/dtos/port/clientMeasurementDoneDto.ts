import { ReturnCode } from "../../enums/returnCode";

export class ClientMeasurementDoneDto {
  ClientIp: string;
  ReturnCode: ReturnCode;
  SorBytes: number[];
}
