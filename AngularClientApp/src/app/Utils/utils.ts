import { OptEventDto } from '../models/dtos/optEventDto';

export class Utils {
  static CompareOptEventDtos(left: OptEventDto, right: OptEventDto): number {
    if (left.eventId > right.eventId) { return -1; }
    if (left.eventId === right.eventId) { return 0; }
    return 1;
  }

  static GetWebApiUrl(): string {
    const protocol = 'http://';
    const port = 11837;
  // const port = 44362;
    return protocol + window.location.hostname + ':' + port;
  }
}
