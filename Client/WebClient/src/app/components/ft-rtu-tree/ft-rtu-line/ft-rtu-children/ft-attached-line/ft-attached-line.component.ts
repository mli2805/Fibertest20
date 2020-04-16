import { Component, OnInit, Input, ViewChild } from "@angular/core";
import { TraceDto } from "src/app/models/dtos/rtuTree/traceDto";
import { MatMenuTrigger } from "@angular/material";
import { Router } from "@angular/router";
import { PortApiService } from "src/app/api/port.service";
import { FtRtuTreeEventService } from "../../../ft-rtu-tree-event-service";
import { FtDetachedTracesProvider } from "src/app/providers/ft-detached-traces-provider";
import { OtauPortDto } from "src/app/models/underlying/otauPortDto";

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
    private portApiService: PortApiService,
    private ftRtuTreeEventService: FtRtuTreeEventService,
    private dataStorage: FtDetachedTracesProvider
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
  assignBaseRefs() {}

  displayStatistics() {
    this.router.navigate(["/trace-statistics", this.trace.traceId]);
  }

  detachTrace() {
    this.ftRtuTreeEventService.emitEvent(true);
    this.portApiService.detachTrace(this.trace.traceId).subscribe((res) => {
      console.log(res);
      this.ftRtuTreeEventService.emitEvent(false);
    });
  }

  outOfTurnMeasurement() {}

  measurementClient() {
    this.prepareData();
    this.router.navigate(["/port-measurement-client"]);
  }

  prepareData() {
    const dict = { rtuId: this.trace.rtuId, otauPortDto: this.trace.otauPort };
    this.dataStorage.data = dict;
  }
}
