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
    const tt = threshold.isAbsolute
      ? this.ts.instant("SID__abs__")
      : this.ts.instant("SID__rel__");

    return `${threshold.value} ${tt}`;
  }

  public eeltStateToScreen(state: boolean): string {
    return state ? this.ts.instant("SID_pass") : this.ts.instant("SID_fail");
  }
}
