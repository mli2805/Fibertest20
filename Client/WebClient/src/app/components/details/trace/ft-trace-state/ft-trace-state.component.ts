import { Component, OnInit, HostListener } from "@angular/core";
import { TraceStateDto } from "src/app/models/dtos/trace/traceStateDto";
import { OneApiService } from "src/app/api/one.service";
import { FiberState } from "src/app/models/enums/fiberState";
import { EventStatus } from "src/app/models/enums/eventStatus";
import { EventStatusPipe } from "src/app/pipes/event-status.pipe";
import { UpdateMeasurementDto } from "src/app/models/dtos/trace/updateMeasurementDto";
import { SignalrService } from "src/app/api/signalr.service";
import { SorFileManager } from "src/app/utils/sorFileManager";
import { Router } from "@angular/router";
import { AccidentPlace } from "src/app/models/enums/accidentPlace";
import { TranslateService } from "@ngx-translate/core";
import { FiberStatePipe } from "src/app/pipes/fiber-state.pipe";

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
  public isButtonDisabled: boolean;

  itemsSourceEventStatuses;
  selectedEventStatus;

  constructor(
    private router: Router,
    private oneApiService: OneApiService,
    private signalRService: SignalrService,
    private eventStatusPipe: EventStatusPipe,
    private ts: TranslateService,
    private fiberStatePipe: FiberStatePipe
  ) {}

  ngOnInit() {
    const ess = Object.keys(EventStatus)
      .filter((e) => !isNaN(+e) && +e > -9)
      .map((e) => {
        return { index: +e, name: this.eventStatusPipe.transform(+e) };
      });
    this.itemsSourceEventStatuses = ess;

    this.isSpinnerVisible = true;
    const params = JSON.parse(sessionStorage.getItem("traceStateParams"));
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

        for (const accidentLine of res.accidents) {
          accidentLine.caption = `${
            accidentLine.number
          } ${this.fiberStatePipe.transform(
            accidentLine.accidentSeriousness
          )} (${accidentLine.accidentTypeLetter}) ${
            accidentLine.accidentPlace === AccidentPlace.InNode
              ? this.ts.instant("SID_in_the_node")
              : this.ts.instant("SID_between_nodes")
          }`;
        }

        this.isSpinnerVisible = false;
      });

    this.signalRService.measurementUpdatedEmitter.subscribe(
      (signal: UpdateMeasurementDto) => {
        if (signal.sorFileId === this.vm.sorFileId) {
          this.vm.eventStatus = signal.eventStatus;
          this.vm.comment = signal.comment;
        }
      }
    );
  }

  showRef() {
    SorFileManager.Show(
      this.router,
      true,
      this.vm.sorFileId,
      "",
      true,
      this.vm.header.traceTitle,
      this.vm.registrationTimestamp
    );
  }

  showRftsEvents() {
    SorFileManager.ShowRftsEvents(this.router, this.vm.sorFileId);
  }

  save() {
    this.isSpinnerVisible = true;
    this.isButtonDisabled = true;

    const dto = this.prepareDto();
    this.oneApiService
      .postRequest("measurement/update", dto)
      .subscribe((res: string) => {
        console.log(res);
      });

    this.isSpinnerVisible = false;
    this.isButtonDisabled = false;
  }

  prepareDto(): UpdateMeasurementDto {
    const dto = new UpdateMeasurementDto();
    dto.sorFileId = this.vm.sorFileId;
    dto.eventStatus = this.selectedEventStatus;
    dto.statusChangedTimestamp = new Date();
    dto.comment = this.vm.comment;
    return dto;
  }

  // it's my button "Back"
  back() {
    console.log(window.history);
    window.history.back();
  }

  @HostListener("window:popstate", ["$event"])
  onPopState(event) {
    const dict = {
      from: "FtTraceStateComponent",
    };
    sessionStorage.setItem("back", JSON.stringify(dict));

    console.log("Back button pressed on FtTraceStateComponent");
    console.log(event);
  }
}
