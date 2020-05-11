import { EventStatus } from "../../enums/eventStatus";

export class UpdateMeasurementDto {
  sorFileId: number;
  eventStatus: EventStatus;
  statusChangedTimestamp: Date;
  statusChangedByUser: string;
  comment: string;
}
