import { RtuNetworkSettingsDto } from "./rtuNetworkSettingsDto";
import { ReturnCode } from "../../enums/returnCode";

export class RtuInitializedWebDto {
  rtuId: string;
  requestId: string;

  returnCode: ReturnCode;
  errorMessage: string;

  rtuNetworkSettings: RtuNetworkSettingsDto;
}
