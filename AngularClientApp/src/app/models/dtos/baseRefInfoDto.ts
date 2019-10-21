import { BaseRefType } from '../enums/baseRefType';

export class BaseRefInfoDto {
  sorFileId: number;
  baseRefType: BaseRefType;
  eventRegistrationTimestamp: Date;
  username: string;
}
