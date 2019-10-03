import { Pipe, PipeTransform } from '@angular/core';
import { MonitoringMode } from '../models/monitoringMode';
import { TranslateService } from '@ngx-translate/core';

@Pipe({
  name: 'monitoringModeToLocalizedStringPipe'
})
export class MonitoringModePipe implements PipeTransform {
  constructor(private ts: TranslateService) {}

  transform(value: MonitoringMode): any {
    switch (value) {
      case MonitoringMode.Off:
        return this.ts.instant('IDS_Manual');
      case MonitoringMode.On:
        return this.ts.instant('IDS_Auto');
    }
    return this.ts.instant('IDS_Unknown');
  }
}
