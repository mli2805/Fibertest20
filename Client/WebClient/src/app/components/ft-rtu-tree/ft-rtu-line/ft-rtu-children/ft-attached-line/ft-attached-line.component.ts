import { Component, OnInit, Input, ViewChild } from "@angular/core";
import { TraceDto } from "src/app/models/dtos/rtuTree/traceDto";
import { MatMenuTrigger } from "@angular/material";
import { Router } from "@angular/router";
import {
  FtRtuTreeEventService,
  RtuTreeEvent,
} from "../../../ft-rtu-tree-event-service";
import { OneApiService } from "src/app/api/one.service";
import { UserDto } from "src/app/models/dtos/userDto";
import { Role } from "src/app/models/enums/role";
import { MonitoringMode } from "src/app/models/enums/monitoringMode";
import { RtuDto } from "src/app/models/dtos/rtuTree/rtuDto";
import { FiberState } from "src/app/models/enums/fiberState";

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

  constructor(
    private router: Router,
    private oneApiService: OneApiService,
    private ftRtuTreeEventService: FtRtuTreeEventService
  ) {}

  ngOnInit() {}

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
    const dict = { rtuId: this.trace.rtuId, otauPortDto: this.trace.otauPort };
    sessionStorage.setItem("measurementClientParams", JSON.stringify(dict));
    this.router.navigate(["/ft-main-nav/port-measurement-client"]);
  }

  isDetachTraceDisabled(): boolean {
    const user: UserDto = JSON.parse(sessionStorage.getItem("currentUser"));
    return user.role > Role.Root || this.trace.isIncludedInMonitoringCycle;
  }

  hasBase(): boolean {
    return !this.trace.hasEnoughBaseRefsToPerformMonitoring;
  }
}
