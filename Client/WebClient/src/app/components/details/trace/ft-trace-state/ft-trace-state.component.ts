import { Component, OnInit } from "@angular/core";
import { TraceStateDto } from "src/app/models/dtos/trace/traceStateDto";
import { OneApiService } from "src/app/api/one.service";
import { FiberState } from "src/app/models/enums/fiberState";
import { EventStatus } from "src/app/models/enums/eventStatus";
import { EventStatusPipe } from "src/app/pipes/event-status.pipe";
import { FtComponentDataProvider } from "src/app/providers/ft-component-data-provider";

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
    private oneApiService: OneApiService,
    private dataStorage: FtComponentDataProvider,
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
    const params = this.dataStorage.data;
    console.log(params);

    this.oneApiService
      .getRequest(`trace/state`, params)
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
