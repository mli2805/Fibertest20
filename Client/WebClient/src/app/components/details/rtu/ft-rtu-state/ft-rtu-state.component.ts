import { Component, OnInit, OnDestroy } from "@angular/core";
import { RtuStateDto } from "src/app/models/dtos/rtu/rtuStateDto";
import { ActivatedRoute } from "@angular/router";
import { OneApiService } from "src/app/api/one.service";
import { Subscription, BehaviorSubject } from "rxjs";
import { SignalrService } from "src/app/api/signalr.service";
import { MonitoringCurrentStep } from "src/app/models/dtos/rtu/currentMonitoringStepDto";
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
  private monitoringStepSubscription: Subscription;
  private monitoringStoppedSubscription: Subscription;
  private monitoringStartedSubscription: Subscription;
  private measurementAddedSubscription: Subscription;

  displayedColumns = [
    "port",
    "traceTitle",
    "traceState",
    "lastMeasId",
    "lastMeasTime",
  ];

  private currentMonitoringStepSubject = new BehaviorSubject<string>("");
  public currentMonitoringStep$ = this.currentMonitoringStepSubject.asObservable();

  clock;

  constructor(
    private activeRoute: ActivatedRoute,
    private oneApiService: OneApiService,
    private signalRService: SignalrService,
    private ts: TranslateService
  ) {}

  ngOnDestroy(): void {
    this.monitoringStepSubscription.unsubscribe();
    this.monitoringStoppedSubscription.unsubscribe();
    this.monitoringStartedSubscription.unsubscribe();
    this.measurementAddedSubscription.unsubscribe();
  }

  async ngOnInit() {
    const id = this.activeRoute.snapshot.paramMap.get("id");

    setInterval(() => {}, 1000);

    const res = (await this.oneApiService
      .getRequest(`rtu/state/${id}`)
      .toPromise()) as RtuStateDto;

    Object.assign(this.vm, res);
    if (this.vm.monitoringMode === MonitoringMode.Off) {
      this.currentMonitoringStepSubject.next(
        this.ts.instant("SID_No_measurement")
      );
    } else {
      this.currentMonitoringStepSubject.next(
        this.ts.instant("SID_Waiting_for_data")
      );
    }

    this.monitoringStepSubscription = this.signalRService.monitoringStepNotifier.subscribe(
      (signal: any) => {
        if (signal.rtuId === this.vm.rtuId) {
          this.notifyUserCurrentMonitoringStep(signal);
        }
      }
    );

    this.monitoringStoppedSubscription = this.signalRService.monitoringStoppedEmitter.subscribe(
      (signal: any) => {
        console.log("monitoring stopped", signal);
        if (signal.rtuId === this.vm.rtuId) {
          console.log("this RTU monitoring stopped", signal);
          this.vm.monitoringMode = MonitoringMode.Off;
          this.currentMonitoringStepSubject.next(
            this.ts.instant("SID_No_measurement")
          );
        }
      }
    );

    this.monitoringStartedSubscription = this.signalRService.monitoringStartedEmitter.subscribe(
      (signal: any) => {
        console.log("monitoring started", signal);
        if (signal.rtuId === this.vm.rtuId) {
          console.log("this RTU monitoring started", signal);
          this.vm.monitoringMode = MonitoringMode.On;
          this.currentMonitoringStepSubject.next(
            this.ts.instant("SID_Waiting_for_data")
          );
        }
      }
    );

    this.measurementAddedSubscription = this.signalRService.measurementAddedEmitter.subscribe(
      (signal: any) => {
        console.log("measurement added", signal);
        if (signal.rtuId === this.vm.rtuId) {
          console.log("this RTU measurement added: ", this.vm.rtuTitle);
          for (const child of this.vm.children) {
            if (child.traceId === signal.traceId) {
              console.log("this TRACE measurement added: ", child.traceTitle);
              child.traceState = signal.traceState;
              child.lastMeasId = signal.sorFileId;
              child.lastMeasTime = signal.registrationTimestamp;
            }
          }
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

    this.currentMonitoringStepSubject.next(
      this.buildMessage(dto.step, portName, traceTitle)
    );
  }

  wrapMeassage(message: string, timestamp: Date) {
    return `${formatDate(timestamp, "HH:mm:ss", "en-US")}  ${message}`;
  }

  buildMessage(
    step: MonitoringCurrentStep,
    portName: string,
    traceTitle: string
  ): string {
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
