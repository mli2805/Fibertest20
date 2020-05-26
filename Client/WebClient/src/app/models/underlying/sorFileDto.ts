import { ReturnCode } from "../enums/returnCode";

export class SorFileDto {
  ClientIp: string;
  ReturnCode: ReturnCode;
  SorBytes: number[];
}
