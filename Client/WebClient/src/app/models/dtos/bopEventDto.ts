export class BopEventDto {
  eventId: number;
  eventRegistrationTimestamp: Date;
  bopAddress: string;
  rtuId: string;
  rtuTitle: string;
  bopState: boolean;
}

export class BopEventRequestDto {
  fullCount: number;
  eventPortion: BopEventDto[];
}
