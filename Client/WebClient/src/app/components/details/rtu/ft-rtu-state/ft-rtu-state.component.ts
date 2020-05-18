import { Component, OnInit, OnDestroy } from "@angular/core";
import { RtuStateDto } from "src/app/models/dtos/rtu/rtuStateDto";
import { ActivatedRoute } from "@angular/router";
import { OneApiService } from "src/app/api/one.service";
import { Subscription } from "rxjs";
import { SignalrService } from "src/app/api/signalr.service";
import {
  CurrentMonitoringStepDto,
  MonitoringCurrentStep,
} from "src/app/models/dtos/rtu/currentMonitoringStepDto";
import { MonitoringMode } from "src/app/models/enums/monitoringMode";
import { TranslateService } from "@ngx-translate/core";
import { formatDate } from "@angular/common";

@Component({
  selector: "ft-rtu-state",
  templateUrl: "./ft-rtu-state.component.html",
  styleUrls: ["./ft-rtu-state.component.css"],
})
export class FtRtuStateComponent implements OnInit, OnDestroy {
  vm: RtuStateDto = new RtuStateDto();
  private subscription: Subscription;

  displayedColumns = [
    "port",
    "traceTitle",
    "traceState",
    "lastMeasId",
    "lastMeasTime",
  ];

  currentMonitoringStep = "waiting for a data...";

  constructor(
    private activeRoute: ActivatedRoute,
    private oneApiService: OneApiService,
    private signalRService: SignalrService,
    private ts: TranslateService
  ) {}

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
    console.log("bye");
  }

  ngOnInit() {
    const id = this.activeRoute.snapshot.paramMap.get("id");
    this.oneApiService
      .getRequest(`rtu/state/${id}`)
      .subscribe((res: RtuStateDto) => {
        Object.assign(this.vm, res);
        if (this.vm.monitoringMode === MonitoringMode.Off) {
          this.currentMonitoringStep = this.ts.instant("SID_No_measurement");
        } else {
          this.currentMonitoringStep = this.ts.instant("SID_Waiting_for_data");
        }
      });

    this.subscription = this.signalRService.monitoringStepNotifier.subscribe(
      (signal: any) => {
        if (signal.rtuId === this.vm.rtuId) {
          this.notifyUserCurrentMonitoringStep(signal);
        }
      }
    );
  }

  notifyUserCurrentMonitoringStep(dto: any) {
    console.log(dto);
    let portName = "";
    let traceTitle = "";

    if (dto.portWithTraceDto != null) {
      portName = dto.portWithTraceDto.otauPort.isPortOnMainCharon
        ? `${dto.portWithTraceDto.otauPort.opticalPort}`
        : `${dto.portWithTraceDto.otauPort.serial}-
                ${dto.portWithTraceDto.otauPort.opticalPort}`;

      const portLineVm = this.vm.children.filter(
        (p) => p.traceId === dto.portWithTraceDto.traceId
      )[0];
      if (portLineVm != null) {
        traceTitle = portLineVm.traceTitle;
        portName = portLineVm.port;
      }
    }

    this.currentMonitoringStep = this.wrapMeassage(
      this.buildMessage(dto.step, portName, traceTitle),
      dto.timestamp
    );
    console.log(this.currentMonitoringStep);
  }

  wrapMeassage(message: string, timestamp: Date) {
    return `${formatDate(timestamp, "HH:mm:ss", "en-US")}  ${message}`;
  }

  buildMessage(
    step: MonitoringCurrentStep,
    portName: string,
    traceTitle: string
  ) {
    switch (step) {
      case MonitoringCurrentStep.Idle:
        return this.ts.instant("SID_No_measurement");
      case MonitoringCurrentStep.Toggle:
        return this.ts.instant("SID_Toggling_to_the_port__0_", portName);
      case MonitoringCurrentStep.Measure:
        return this.ts.instant("SID_Measurement_on_port__0___trace___1__", {
          0: portName,
          1: traceTitle,
        });
      case MonitoringCurrentStep.Analysis:
        return this.ts.instant(
          "SID_Measurement_s_result_analysis__port__0____trace___1__",
          { 0: portName, 1: traceTitle }
        );
      case MonitoringCurrentStep.Interrupted:
        return this.ts.instant("SID_Measurement_interrupted");
      default:
        return this.ts.instant("SID_Unknown");
    }
  }
}
