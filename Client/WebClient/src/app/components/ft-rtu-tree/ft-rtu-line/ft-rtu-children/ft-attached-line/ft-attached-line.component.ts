import { Component, OnInit, Input, ViewChild } from "@angular/core";
import { TraceDto } from "src/app/models/dtos/rtuTree/traceDto";
import { MatMenuTrigger } from "@angular/material";
import { Router } from "@angular/router";
import { FtRtuTreeEventService } from "../../../ft-rtu-tree-event-service";
import { FtComponentDataProvider } from "src/app/providers/ft-component-data-provider";
import { OneApiService } from "src/app/api/one.service";

@Component({
  selector: "ft-attached-line",
  templateUrl: "./ft-attached-line.component.html",
  styleUrls: ["./ft-attached-line.component.scss"],
})
export class FtAttachedLineComponent implements OnInit {
  @Input() trace: TraceDto;

  @ViewChild(MatMenuTrigger, null)
  contextMenu: MatMenuTrigger;
  contextMenuPosition = { x: "0px", y: "0px" };

  constructor(
    private router: Router,
    private oneApiService: OneApiService,
    private ftRtuTreeEventService: FtRtuTreeEventService,
    private dataStorage: FtComponentDataProvider
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
    this.prepareDataForAssignBaseRefs();
    this.router.navigate(["/assign-base", this.trace.traceId]);
  }

  displayState() {
    this.router.navigate(["/trace-state", this.trace.traceId]);
  }

  displayStatistics() {
    this.router.navigate(["/trace-statistics", this.trace.traceId]);
  }

  detachTrace() {
    this.ftRtuTreeEventService.emitEvent(true);
    this.oneApiService
      .postRequest("port", "detach-trace", this.trace.traceId)
      .subscribe((res) => {
        console.log(res);
        this.ftRtuTreeEventService.emitEvent(false);
      });
  }

  outOfTurnMeasurement() {}

  measurementClient() {
    this.prepareDataForMeasurementClient();
    this.router.navigate(["/port-measurement-client"]);
  }

  prepareDataForAssignBaseRefs() {
    const dict = { trace: this.trace };
    this.dataStorage.data = dict;
  }
  prepareDataForMeasurementClient() {
    const dict = { rtuId: this.trace.rtuId, otauPortDto: this.trace.otauPort };
    this.dataStorage.data = dict;
  }
}
