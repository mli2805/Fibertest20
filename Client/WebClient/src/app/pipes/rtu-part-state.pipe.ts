import { Pipe, PipeTransform } from "@angular/core";
import { TranslateService } from "@ngx-translate/core";
import { RtuPartState } from "../models/enums/rtuPartState";

@Pipe({
  name: "rtuPartStateToLocalizedStringPipe"
})
export class RtuPartStatePipe implements PipeTransform {
  constructor(private ts: TranslateService) {}

  transform(value: RtuPartState): string {
    switch (value) {
      case RtuPartState.Broken:
        return this.ts.instant("SID_Broken");
      case RtuPartState.Ok:
        return this.ts.instant("SID_Ok");
      default:
        return "";
    }
  }
}
