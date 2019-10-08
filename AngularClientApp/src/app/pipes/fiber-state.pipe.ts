import { Pipe, PipeTransform } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { FiberState } from '../models/enums/fiberState';

@Pipe({
  name: 'fiberStateToLocalizedStringPipe'
})
export class FiberStatePipe implements PipeTransform {
  constructor(private ts: TranslateService) {}

  transform(value: FiberState): string {
    switch (value) {
      case FiberState.NotInTrace:
        return this.ts.instant('SID_Not_in_trace');
      case FiberState.NotJoined:
        return this.ts.instant('SID_Not_joined');
      case FiberState.DistanceMeasurement:
        return this.ts.instant('SID_Distace_measurement');
      case FiberState.Unknown:
        return this.ts.instant('SID_Unknown');
      case FiberState.Ok:
        return this.ts.instant('SID_Ok');
      case FiberState.Suspicion:
        return this.ts.instant('SID_Suspicion');
      case FiberState.Minor:
        return this.ts.instant('SID_Minor');
      case FiberState.Major:
        return this.ts.instant('SID_Major');
      case FiberState.User:
        return this.ts.instant('SID_User_s_threshold');
      case FiberState.Critical:
        return this.ts.instant('SID_Critical');
      case FiberState.FiberBreak:
        return this.ts.instant('SID_fiber_break');
      case FiberState.NoFiber:
        return this.ts.instant('SID_No_fiber');
      case FiberState.HighLighted:
        return this.ts.instant('SID_Highlighted');
      default:
        return this.ts.instant('SID_Ok');
    }
  }
}
