import { BaseRefType } from '../enums/baseRefType';

export class BaseRefInfoDto {
  sorFileId: number;
  baseRefType: BaseRefType;
  assignmentTimestamp: Date;
  username: string;
}
