import { Component, OnInit, Input, ViewChild } from "@angular/core";
import { TraceDto } from "src/app/models/dtos/rtuTree/traceDto";
import { MatMenuTrigger } from "@angular/material";
import { Router } from "@angular/router";
import {
  FtRtuTreeEventService,
  RtuTreeEvent,
} from "../../../ft-rtu-tree-event-service";
import { OneApiService } from "src/app/api/one.service";
import { RegistrationAnswerDto } from "src/app/models/dtos/registrationAnswerDto";
import { Role } from "src/app/models/enums/role";
import { RtuDto } from "src/app/models/dtos/rtuTree/rtuDto";
import { FiberState } from "src/app/models/enums/fiberState";
import { MonitoringMode } from "src/app/models/enums/monitoringMode";
import { RtuPartState } from "src/app/models/enums/rtuPartState";

@Component({
  selector: "ft-attached-line",
  templateUrl: "./ft-attached-line.component.html",
  styleUrls: ["./ft-attached-line.component.scss"],
})
export class FtAttachedLineComponent implements OnInit {
  @Input() parentRtu: RtuDto;
  @Input() trace: TraceDto;

  @ViewChild(MatMenuTrigger, null)
  contextMenu: MatMenuTrigger;
  contextMenuPosition = { x: "0px", y: "0px" };

  user: RegistrationAnswerDto;
 
  constructor(
    private router: Router,
    private oneApiService: OneApiService,
    private ftRtuTreeEventService: FtRtuTreeEventService
  ) {}

  ngOnInit() {
    this.user = JSON.parse(sessionStorage.getItem("currentUser"));
    console.log(`I'm trace on port ${this.trace.otauPort.opticalPort} of mainOtau ${this.trace.otauPort.isPortOnMainCharon} with otauId ${this.trace.otauPort.otauId}. Trace title is: ${this.trace.title}`)
  }

  onContextMenu(event: MouseEvent) {
    event.preventDefault();
    this.contextMenuPosition.x = event.clientX + "px";
    this.contextMenuPosition.y = event.clientY + "px";
    this.contextMenu.menuData = { item: this.trace.title };
    this.contextMenu.openMenu();
    this.contextMenu.focus("mouse");
  }

  displayInformation() {
    this.router.navigate([
      "/ft-main-nav/trace-information",
      this.trace.traceId,
    ]);
  }
  assignBaseRefs() {
    const dict = { trace: this.trace };
    sessionStorage.setItem("assignBaseParams", JSON.stringify(dict));
    this.router.navigate(["/ft-main-nav/assign-base", this.trace.traceId]);
  }

  displayState() {
    if (this.trace.state === FiberState.Unknown) {
      return;
    }
    const dict = {
      type: "traceId",
      traceId: this.trace.traceId,
      fileId: null,
    };
    sessionStorage.setItem("traceStateParams", JSON.stringify(dict));
    this.router.navigate(["/ft-main-nav/trace-state"]);
  }

  displayStatistics() {
    this.router.navigate(["/ft-main-nav/trace-statistics", this.trace.traceId]);
  }

  displayLandmarks() {
    this.router.navigate(["/ft-main-nav/trace-landmarks", this.trace.traceId]);
  }

  async detachTrace() {
    this.ftRtuTreeEventService.emitEvent(RtuTreeEvent.showSpinner);
    await this.oneApiService
      .postRequest(`port/detach-trace/${this.trace.traceId}`, null)
      .toPromise();
    console.log(`detach trace: done`);
    // server will send fetch signal
    // this.ftRtuTreeEventService.emitEvent(RtuTreeEvent.fetchTree);
  }

  outOfTurnMeasurement() {
    const dict = {
      trace: this.trace,
      rtu: this.parentRtu,
    };
    sessionStorage.setItem("outOfTurnMeasurementParams", JSON.stringify(dict));
    this.router.navigate(["/ft-main-nav/out-of-turn-measurement"]);
  }

  measurementClient() {
    const dict = { 
      selectedRtu: this.parentRtu,
      selectedPort: this.trace.otauPort 
    };
    sessionStorage.setItem("measurementClientParams", JSON.stringify(dict));
    this.router.navigate(["/ft-main-nav/port-measurement-client"]);
  }

  isDetachTraceDisabled(): boolean {
     return (
      this.user.role > Role.WebOperator ||
      (this.trace.isIncludedInMonitoringCycle &&
        this.trace.rtuMonitoringMode === MonitoringMode.On) || !this.isRtuAvailable()
    );
  }

  isOutOfTurnPreciseDisabled(): boolean {
     return (
      this.user.role > Role.WebOperator ||
      !this.trace.hasEnoughBaseRefsToPerformMonitoring || !this.isRtuAvailable()
    );
  }

  isMeasurementClientDisabled(): boolean {
        return (this.user.role > Role.WebOperator || !this.isRtuAvailable());
  }

  hasBase(): boolean {
    return !this.trace.hasEnoughBaseRefsToPerformMonitoring;
  }

  isRtuAvailable() : boolean {
    return this.parentRtu.mainChannelState === RtuPartState.Ok || this.parentRtu.reserveChannelState === RtuPartState.Ok;
  }
}
