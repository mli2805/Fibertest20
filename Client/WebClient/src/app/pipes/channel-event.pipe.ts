import { Pipe, PipeTransform } from "@angular/core";
import { TranslateService } from "@ngx-translate/core";
import { ChannelEvent } from "../models/enums/channelEvent";

@Pipe({
  name: "channelEventToLocalizedStringPipe",
})
export class ChannelEventPipe implements PipeTransform {
  constructor(private ts: TranslateService) {}

  transform(value) {
    switch (value) {
      case ChannelEvent.Broken:
        return this.ts.instant("SID_Broken");
      case ChannelEvent.Nothing:
        return "";
      case ChannelEvent.Repaired:
        return this.ts.instant("SID_Repaired");
    }
  }
}
