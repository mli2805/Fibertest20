import { ChannelEvent } from "../enums/channelEvent";

export class NetworkEventDto {
  eventId: number;
  eventRegistrationTimestamp: Date;
  rtuTitle: string;

  isRtuAvailable: boolean;
  mainChannelEvent: ChannelEvent;
  reserveChannelEvent: ChannelEvent;
}

export class NetworkEventRequestDto {
  fullCount: number;
  eventPortion: NetworkEventDto[];
}
