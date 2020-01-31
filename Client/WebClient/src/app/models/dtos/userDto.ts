import { ReturnCode } from "../enums/returnCode";

export class UserDto {
  returnCode: ReturnCode;
  username: string;
  role: string;
  zone: string;
  jsonWebToken: string;
}
