import { Component, OnInit, Input } from "@angular/core";
import { TranslateService } from "@ngx-translate/core";
import {
  RftsEventsDto,
  LevelState,
} from "src/app/models/dtos/trace/rftsEventsDto";
import { OneApiService } from "src/app/api/one.service";
import { ActivatedRoute } from "@angular/router";

@Component({
  selector: "ft-rfts-events",
  templateUrl: "./ft-rfts-events.component.html",
  styleUrls: ["./ft-rfts-events.component.css"],
})
export class FtRftsEventsComponent implements OnInit {
  public vm: RftsEventsDto = new RftsEventsDto();
  public isSpinnerVisible: boolean;
  public isNoFiberVisible: boolean;
  public isTableVisible: boolean;

  constructor(
    private activeRoute: ActivatedRoute,
    private oneApiService: OneApiService,
    private ts: TranslateService
  ) {}

  async ngOnInit() {
    this.isSpinnerVisible = true;

    const sorFileId = this.activeRoute.snapshot.paramMap.get("id");
    await this.getData(sorFileId);

    this.isSpinnerVisible = false;
    this.isNoFiberVisible = this.vm.isNoFiber;
    this.isTableVisible = !this.vm.isNoFiber;
  }

  async getData(sorFileId: string) {
    try {
      this.vm = (await this.oneApiService
        .getRequest(`sor/rfts-events/${sorFileId}`)
        .toPromise()) as RftsEventsDto;
      console.log("vm: ", this.vm);

      if (!this.vm.isNoFiber) {
        this.evaluateResults(this.vm);
      }
    } catch (err) {
      console.log(err);
    }
  }

  evaluateResults(res: RftsEventsDto) {
    res.summary.levelStates = new Array();
    res.summary.traceState = "SID_Ok";
    for (const level of res.levelArray) {
      const levelState = new LevelState();
      levelState.levelTitle = level.title;
      levelState.state = level.isFailed
        ? this.ts.instant("SID_fail___0__km_", {
            0: level.firstProblemLocation,
          })
        : this.ts.instant("SID_pass");
      res.summary.levelStates.push(levelState);
      if (level.isFailed) {
        res.summary.traceState = level.title;
      }
    }
  }
}
