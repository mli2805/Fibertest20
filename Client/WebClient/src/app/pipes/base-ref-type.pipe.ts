import { Pipe, PipeTransform } from "@angular/core";
import { TranslateService } from "@ngx-translate/core";
import { BaseRefType } from "../models/enums/baseRefType";

@Pipe({
  name: "BaseRefStringPipe"
})
export class BaseRefPipe implements PipeTransform {
  constructor() {}

  transform(value: BaseRefType): string {
    switch (value) {
      case BaseRefType.Precise:
        return ("Precise");
      case BaseRefType.Fast:
        return ("Fast");
      case BaseRefType.Additional:
        return ("Second");
      default:
        return "";
    }
  }
}

@Pipe({
  name: "BaseRefTypeToLocalizedStringPipe"
})
export class BaseRefTypePipe implements PipeTransform {
  constructor(private ts: TranslateService) {}

  transform(value: BaseRefType): string {
    switch (value) {
      case BaseRefType.Precise:
        return this.ts.instant("SID_Precise");
      case BaseRefType.Fast:
        return this.ts.instant("SID_Fast");
      case BaseRefType.Additional:
        return this.ts.instant("SID_Second");
      default:
        return "";
    }
  }
}

@Pipe({
  name: "BaseRefTypeToLocalizedFemaleStringPipe"
})
export class BaseRefTypeFemalePipe implements PipeTransform {
  constructor(private ts: TranslateService) {}

  transform(value: BaseRefType): string {
    switch (value) {
      case BaseRefType.Precise:
        return this.ts.instant("SID_PreciseF");
      case BaseRefType.Fast:
        return this.ts.instant("SID_FastF");
      case BaseRefType.Additional:
        return this.ts.instant("SID_SecondF");
      default:
        return "";
    }
  }
}
