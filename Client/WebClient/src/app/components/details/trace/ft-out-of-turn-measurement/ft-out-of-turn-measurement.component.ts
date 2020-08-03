import { Component, OnInit } from "@angular/core";
import { DoOutOfTurnMeasurementDto } from "src/app/models/dtos/trace/doOutOfTurnMeasurementDto";
import { PortWithTraceDto } from "src/app/models/underlying/portWithTraceDto";
import { OneApiService } from "src/app/api/one.service";
import { TraceHeaderDto } from "src/app/models/dtos/trace/traceHeaderDto";
import { TranslateService } from "@ngx-translate/core";
import { ReturnCode } from "src/app/models/enums/returnCode";
import { SignalrService } from "src/app/api/signalr.service";
import { TraceStateDto } from "src/app/models/dtos/trace/traceStateDto";
import { Router } from "@angular/router";

@Component({
  selector: "ft-out-of-turn-measurement",
  templateUrl: "./ft-out-of-turn-measurement.component.html",
  styleUrls: ["./ft-out-of-turn-measurement.component.css"],
})
export class FtOutOfTurnMeasurementComponent implements OnInit {
  public isSpinnerVisible: boolean;
  public vm: TraceHeaderDto = new TraceHeaderDto();
  public params: any;
  message;

  constructor(
    private router: Router,
    private oneApiService: OneApiService,
    private signalRService: SignalrService,
    private ts: TranslateService
  ) {}

  ngOnInit() {
    this.isSpinnerVisible = true;

    setInterval(() => {}, 1000);

    this.params = JSON.parse(
      sessionStorage.getItem("outOfTurnMeasurementParams")
    );
    console.log(this.params);

    this.signalRService.measurementAddedEmitter.subscribe(
      (signal: TraceStateDto) => {
        console.log(signal);
        const dict = {
          type: "traceId",
          traceId: this.params.trace.traceId,
          fileId: null,
        };
        sessionStorage.setItem("traceStateParams", JSON.stringify(dict));
        this.isSpinnerVisible = false;
        this.router.navigate(["/trace-state"]);
      }
    );

    this.sendCommand(this.params);
  }

  private sendCommand(params: any) {
    this.vm.traceTitle = params.trace.title;
    this.vm.port = params.trace.otauPort.opticalPort;
    this.vm.rtuTitle = params.rtu.title;

    this.message = this.ts.instant("SID_Sending_command__Wait_please___");

    const dto = new DoOutOfTurnMeasurementDto();
    dto.rtuId = params.rtu.rtuId;
    dto.portWithTraceDto = new PortWithTraceDto();
    dto.portWithTraceDto.traceId = params.trace.traceId;
    dto.portWithTraceDto.otauPort = params.trace.otauPort;
    this.oneApiService
      .postRequest("measurement/out-of-turn-measurement", dto)
      .subscribe((res: any) => {
        console.log(res);
        if (res.returnCode !== ReturnCode.Ok) {
          this.message = res.errorMessage;
          this.isSpinnerVisible = false;
        } else {
          this.message = this.ts.instant("SID_Precise_monitoring_in_progress_");
        }
      });
  }
}
