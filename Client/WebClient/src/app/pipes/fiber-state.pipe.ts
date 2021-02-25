import { Pipe, PipeTransform } from "@angular/core";
import { TranslateService } from "@ngx-translate/core";
import { BaseRefType } from "../models/enums/baseRefType";
import { FiberState } from "../models/enums/fiberState";

@Pipe({
  name: "fiberStateToLocalizedStringPipe",
})
export class FiberStatePipe implements PipeTransform {
  constructor(private ts: TranslateService) {}

  transform(
    value: FiberState,
    arg1: BaseRefType = BaseRefType.Precise
  ): string {
    if (value <= FiberState.Ok) {
      switch (value) {
        case FiberState.Nothing:
          return "";
        case FiberState.NotInTrace:
          return this.ts.instant("SID_Not_in_trace");
        case FiberState.NotJoined:
          return this.ts.instant("SID_Not_joined");
        case FiberState.DistanceMeasurement:
          return this.ts.instant("SID_Distace_measurement");
        case FiberState.Unknown:
          return this.ts.instant("SID_Unknown");
        case FiberState.Ok:
          return this.ts.instant("SID_Ok");
        default:
          return this.ts.instant("SID_Ok");
      }
    } else {
      if (arg1 === BaseRefType.Fast) {
        return this.ts.instant("SID_Suspicion");
      } else {
        switch (value) {
          case FiberState.Minor:
            return this.ts.instant("SID_Minor");
          case FiberState.Major:
            return this.ts.instant("SID_Major");
          case FiberState.User:
            return this.ts.instant("SID_User_s_threshold");
          case FiberState.Critical:
            return this.ts.instant("SID_Critical");
          case FiberState.FiberBreak:
            return this.ts.instant("SID_fiber_break");
          case FiberState.NoFiber:
            return this.ts.instant("SID_No_fiber");
          case FiberState.HighLighted:
            return this.ts.instant("SID_Highlighted");
          default:
            return this.ts.instant("SID_Ok");
        }
      }
    }
  }
}
