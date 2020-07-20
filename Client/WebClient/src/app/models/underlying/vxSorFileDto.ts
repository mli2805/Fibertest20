import { ReturnCode } from "../enums/returnCode";

export class VxSorFileDto {
  returnCode: ReturnCode;
  protobuf: Uint8Array;
}
