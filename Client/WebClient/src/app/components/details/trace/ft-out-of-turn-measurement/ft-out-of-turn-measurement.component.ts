import { Component, OnInit, HostListener, OnDestroy } from "@angular/core";
import { DoOutOfTurnMeasurementDto } from "src/app/models/dtos/trace/doOutOfTurnMeasurementDto";
import { PortWithTraceDto } from "src/app/models/underlying/portWithTraceDto";
import { OneApiService } from "src/app/api/one.service";
import { TraceHeaderDto } from "src/app/models/dtos/trace/traceHeaderDto";
import { TranslateService } from "@ngx-translate/core";
import { ReturnCode } from "src/app/models/enums/returnCode";
import { SignalrService } from "src/app/api/signalr.service";
import { TraceStateDto } from "src/app/models/dtos/trace/traceStateDto";
import { Router } from "@angular/router";
import { RequestAnswer } from "src/app/models/underlying/requestAnswer";
import { OccupyRtuDto, RtuOccupation, RtuOccupationState } from "src/app/models/dtos/meas-params/occupyRtuDto";
import { ReturnCodePipe } from "src/app/pipes/return-code.pipe";

@Component({
  selector: "ft-out-of-turn-measurement",
  templateUrl: "./ft-out-of-turn-measurement.component.html",
  styleUrls: ["./ft-out-of-turn-measurement.component.css"],
})
export class FtOutOfTurnMeasurementComponent implements OnInit, OnDestroy {
  public isSpinnerVisible: boolean;
  public vm: TraceHeaderDto = new TraceHeaderDto();
  public params: any;
  message;

  measEmmitterSubscription;

  constructor(
    private router: Router,
    private oneApiService: OneApiService,
    private signalRService: SignalrService,
    private returnCodePipe: ReturnCodePipe,
    private ts: TranslateService
  ) {}

  ngOnInit() {
    const back = sessionStorage.getItem("back");
    console.log(back);
    if (back !== null) {
      sessionStorage.removeItem("back");
      window.history.back();
      return;
    }

    this.isSpinnerVisible = true;

    this.params = JSON.parse(
      sessionStorage.getItem("outOfTurnMeasurementParams")
    );
    console.log(this.params);

    this.measEmmitterSubscription = this.signalRService.measurementAddedEmitter.subscribe(
      (signal: TraceStateDto) => {
        console.log(signal);
        const dict = {
          type: "traceId",
          traceId: this.params.trace.traceId,
          fileId: null,
        };
        sessionStorage.setItem("traceStateParams", JSON.stringify(dict));
        this.isSpinnerVisible = false;
        this.router.navigate(["/ft-main-nav/trace-state"]);
      }
    );

    this.sendCommand(this.params);
  }

  async ngOnDestroy() {
    var freeDto = new OccupyRtuDto();
    freeDto.rtuId = this.params.rtu.rtuId;
    freeDto.state = new RtuOccupationState();
    freeDto.state.rtuId = this.params.rtu.rtuId;
    freeDto.state.rtuOccupation = RtuOccupation.None;

    const res = (await this.oneApiService
      .postRequest("rtu/set-rtu-occupation-state", freeDto)
      .toPromise()) as RequestAnswer;
    console.log(`${this.returnCodePipe.transform(res.returnCode)}`);

    this.measEmmitterSubscription.unsubscribe();
  }

  private async sendCommand(params: any) {
    this.vm.traceTitle = params.trace.title;
    this.vm.port = params.trace.otauPort.opticalPort;
    this.vm.rtuTitle = params.rtu.title;

    this.message = this.ts.instant("SID_Sending_command__Wait_please___");

    const dto = new DoOutOfTurnMeasurementDto();
    dto.rtuId = params.rtu.rtuId;
    dto.portWithTraceDto = new PortWithTraceDto();
    dto.portWithTraceDto.traceId = params.trace.traceId;
    dto.portWithTraceDto.otauPort = params.trace.otauPort;
    const res = (await this.oneApiService
      .postRequest("measurement/out-of-turn-measurement", dto)
      .toPromise()) as RequestAnswer;
    console.log(res);
    if (res.returnCode !== ReturnCode.Ok) {
      this.message = this.returnCodePipe.transform(res.returnCode);
      this.isSpinnerVisible = false;
    } else {
      this.message = this.ts.instant("SID_Precise_monitoring_in_progress_");
    }
  }

  @HostListener("window:popstate", ["$event"])
  onPopState(event) {
    console.log("Back button pressed on FtOutOfTurnMeasurementComponent");
    console.log(event);
  }
}
