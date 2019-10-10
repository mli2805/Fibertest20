import { OptEventDto } from '../models/dtos/optEventDto';

export class Utils {
  static CompareOptEventDtos(left: OptEventDto, right: OptEventDto): number {
    if (left.eventId > right.eventId) { return -1; }
    if (left.eventId === right.eventId) { return 0; }
    return 1;
  }
}
