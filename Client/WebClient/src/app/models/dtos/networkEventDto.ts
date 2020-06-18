import { ChannelEvent } from "../enums/channelEvent";

export class NetworkEventDto {
  eventId: number;
  eventRegistrationTimestamp: Date;
  rtuId: string;
  rtuTitle: string;

  isRtuAvailable: boolean;
  onMainChannel: ChannelEvent;
  onReserveChannel: ChannelEvent;
}

export class NetworkEventRequestDto {
  fullCount: number;
  eventPortion: NetworkEventDto[];
}
