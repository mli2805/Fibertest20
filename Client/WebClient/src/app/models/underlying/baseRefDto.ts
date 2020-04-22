import { BaseRefType } from "../enums/baseRefType";

export class BaseRefDto {
  id: string;
  baseRefType: BaseRefType;
  userName: string;
  saveTimestamp: number;
  duration: number;
  sorFileId: number;
  sorBytes;
}
