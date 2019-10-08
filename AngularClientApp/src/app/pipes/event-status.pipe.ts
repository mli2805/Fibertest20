import { Pipe, PipeTransform } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { EventStatus } from '../models/enums/eventStatus';

@Pipe({
  name: 'eventStatusToLocalizedStringPipe'
})
export class EventStatusPipe implements PipeTransform {
  constructor(private ts: TranslateService) {}

  transform(value: EventStatus): string {
    switch (value) {
      case EventStatus.JustMeasurementNotAnEvent:
      case EventStatus.EventButNotAnAccident:
        return '';
      case EventStatus.NotImportant:
        return this.ts.instant('SID_Not_important');
      case EventStatus.Planned:
        return this.ts.instant('SID_Planned');
      case EventStatus.NotConfirmed:
        return this.ts.instant('SID_Not_confirmed');
      case EventStatus.Unprocessed:
        return this.ts.instant('SID_Unprocessed');
      case EventStatus.Suspended:
        return this.ts.instant('SID_Suspended');
      case EventStatus.Confirmed:
        return this.ts.instant('SID_Confirmed');
      default:
        return this.ts.instant('SID_Unprocessed');
    }
  }
}
