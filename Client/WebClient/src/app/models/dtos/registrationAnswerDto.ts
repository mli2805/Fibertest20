import { ReturnCode } from "../enums/returnCode";
import { Role } from "../enums/role";

export class RegistrationAnswerDto {
  returnCode: ReturnCode;
  username: string;
  role: Role;
  zone: string;
  connectionId: string;
  jsonWebToken: string;
  serverVersion: string;
}
