import { UnRegisterReason } from "../enums/unRegisterReason";

export class ServerAsksClientToExitDto {
  toAll: boolean;
  connectionId: string;
  reason: UnRegisterReason;

  newAddress: string;
  isNewUserWeb: boolean;
}
