import { Component, OnInit, Input } from "@angular/core";
import {
  RftsLevelDto,
  MonitoringThreshold,
} from "src/app/models/dtos/trace/rftsEventsDto";
import { TranslateService } from "@ngx-translate/core";

@Component({
  selector: "ft-rfts-events-level",
  templateUrl: "./ft-rfts-events-level.component.html",
  styleUrls: ["./ft-rfts-events-level.component.css"],
})
export class FtRftsEventsLevelComponent implements OnInit {
  @Input() vm: RftsLevelDto;
  constructor(private ts: TranslateService) {}

  ngOnInit() {}

  public thresholdToScreen(threshold: MonitoringThreshold): string {
    if (threshold === null) {
      return "";
    }
    const tt = threshold.isAbsolute
      ? this.ts.instant("SID__abs__")
      : this.ts.instant("SID__rel__");

    return `${(threshold.value / 1000).toFixed(3)} ${tt}`;
  }

  public eeltStateToScreen(state: boolean): string {
    return state ? this.ts.instant("SID_pass") : this.ts.instant("SID_fail");
  }

  public getStateBackgroundColor(eventStateString: string) {
    if (eventStateString !== "" && eventStateString !== "SID_pass") {
      return "red";
    }
    return "transparent";
  }
}
