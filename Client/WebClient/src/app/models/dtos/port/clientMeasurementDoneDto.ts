import { ReturnCode } from "../../enums/returnCode";
import { RtuMaker } from "../../enums/rtuMaker";

export class ClientMeasurementDoneDto {
  connectionId: string;
  clientIp: string;
  returnCode: ReturnCode;
  sorBytes: number[];
  clientMeasurementId: string;
  rtuMaker: RtuMaker;
}
