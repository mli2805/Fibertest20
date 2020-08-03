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
import { DoOutOfTurnMeasurementDto } from "src/app/models/dtos/trace/doOutOfTurnMeasurementDto";
import { PortWithTraceDto } from "src/app/models/underlying/portWithTraceDto";

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
    this.router.navigate(["/trace-information", this.trace.traceId]);
  }
  assignBaseRefs() {
    const dict = { trace: this.trace };
    sessionStorage.setItem("assignBaseParams", JSON.stringify(dict));
    this.router.navigate(["/assign-base", this.trace.traceId]);
  }

  displayState() {
    const dict = {
      type: "traceId",
      traceId: this.trace.traceId,
      fileId: null,
    };
    sessionStorage.setItem("traceStateParams", JSON.stringify(dict));
    this.router.navigate(["/trace-state"]);
  }

  displayStatistics() {
    this.router.navigate(["/trace-statistics", this.trace.traceId]);
  }

  detachTrace() {
    this.ftRtuTreeEventService.emitEvent(RtuTreeEvent.showSpinner);
    this.oneApiService
      .postRequest(`port/detach-trace/${this.trace.traceId}`, null)
      .subscribe((res) => {
        console.log(res);
        this.ftRtuTreeEventService.emitEvent(RtuTreeEvent.fetchTree);
      });
  }

  outOfTurnMeasurement() {
    const dict = {
      trace: this.trace,
      rtu: this.parentRtu,
    };
    sessionStorage.setItem("outOfTurnMeasurementParams", JSON.stringify(dict));
    this.router.navigate(["/out-of-turn-measurement"]);
  }

  measurementClient() {
    const dict = { rtuId: this.trace.rtuId, otauPortDto: this.trace.otauPort };
    sessionStorage.setItem("measurementClientParams", JSON.stringify(dict));
    this.router.navigate(["/port-measurement-client"]);
  }

  isDetachTraceDisabled(): boolean {
    const user: UserDto = JSON.parse(sessionStorage.getItem("currentUser"));
    return (
      user.role > Role.Root ||
      this.parentRtu.monitoringMode === MonitoringMode.On
    );
  }

  hasBase(): boolean {
    return !this.trace.hasEnoughBaseRefsToPerformMonitoring;
  }
}
