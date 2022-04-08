import { ReturnCode } from "../../enums/returnCode";
import { RtuMaker } from "../../enums/rtuMaker";

export class ClientMeasurementDoneDto {
  connectionId: string;
  clientIp: string;
  returnCode: ReturnCode;
  sorBytes: number[];
  id: string;
  rtuMaker: RtuMaker;
}
