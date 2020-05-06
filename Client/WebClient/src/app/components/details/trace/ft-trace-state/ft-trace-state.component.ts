import { Component, OnInit } from "@angular/core";
import { TraceStateDto } from "src/app/models/dtos/trace/traceStateDto";
import { OneApiService } from "src/app/api/one.service";
import { ActivatedRoute } from "@angular/router";
import { TranslateService } from "@ngx-translate/core";
import { FiberState } from "src/app/models/enums/fiberState";
import { EventStatus } from "src/app/models/enums/eventStatus";
import { EventStatusPipe } from "src/app/pipes/event-status.pipe";

@Component({
  selector: "ft-trace-state",
  templateUrl: "./ft-trace-state.component.html",
  styleUrls: ["./ft-trace-state.component.css"],
})
export class FtTraceStateComponent implements OnInit {
  vm: TraceStateDto = new TraceStateDto();
  public isAccidentsVisible: boolean;
  public isEventStatusVisible: boolean;
  public isSpinnerVisible: boolean;

  itemsSourceEventStatuses;
  selectedEventStatus;

  constructor(
    private activeRoute: ActivatedRoute,
    private oneApiService: OneApiService,
    private ts: TranslateService,
    private eventStatusPipe: EventStatusPipe
  ) {}

  ngOnInit() {
    const ess = Object.keys(EventStatus)
      .filter((e) => !isNaN(+e) && +e > -9)
      .map((e) => {
        return { index: +e, name: this.eventStatusPipe.transform(+e) };
      });
    this.itemsSourceEventStatuses = ess;

    this.isSpinnerVisible = true;
    const id = this.activeRoute.snapshot.paramMap.get("id");
    this.oneApiService
      .getRequest(`trace/state/${id}`)
      .subscribe((res: TraceStateDto) => {
        console.log(res);
        this.vm = res;
        this.isAccidentsVisible =
          res.traceState !== FiberState.Ok &&
          res.traceState !== FiberState.NoFiber;
        this.isEventStatusVisible =
          res.eventStatus > EventStatus.EventButNotAnAccident;
        this.selectedEventStatus = res.eventStatus;
        this.isSpinnerVisible = false;
      });
  }
}
