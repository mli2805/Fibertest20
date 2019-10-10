import { BaseRefType } from '../enums/baseRefType';
import { FiberState } from '../enums/fiberState';
import { EventStatus } from '../enums/eventStatus';

export class OptEventDto {
  eventId: number;
  measurementTimestamp: Date;
  eventRegistrationTimestamp: Date;
  rtuTitle: string;
  traceTitle: string;

  baseRefType: BaseRefType;
  traceState: FiberState;

  eventStatus: EventStatus;
  statusChangedTimestamp: Date;
  statusChangedByUser: string;

  comment: string;
}
