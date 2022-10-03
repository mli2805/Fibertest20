import { RtuNetworkSettingsDto } from "./rtuNetworkSettingsDto";
import { ReturnCode } from "../../enums/returnCode";

export class RtuInitializedWebDto {
  rtuId: string;

  returnCode: ReturnCode;
  errorMessage: string;

  rtuNetworkSettings: RtuNetworkSettingsDto;
}

export class InitializeRtuDto {
  connectionId: string;
  rtuId: string;

  // the rest will be filled in on server
}
