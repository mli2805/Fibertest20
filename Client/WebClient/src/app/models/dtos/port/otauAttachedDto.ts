import { ReturnCode } from "../../enums/returnCode";

export class OtauAttachedDto {
  otauId: string;

  rtuId: string;
  isAttached: boolean;

  returnCode: ReturnCode;
  errorMessage: string;
  serial: string;
  portCount: number;
}
