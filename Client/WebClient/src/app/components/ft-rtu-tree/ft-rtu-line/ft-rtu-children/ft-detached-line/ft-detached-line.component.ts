import { Component, OnInit, Input, ViewChild } from "@angular/core";
import { TraceDto } from "src/app/models/dtos/rtuTree/traceDto";
import { MatMenuTrigger } from "@angular/material";
import { Router } from "@angular/router";
import { FtComponentDataProvider } from "src/app/providers/ft-component-data-provider";

@Component({
  selector: "ft-detached-line",
  templateUrl: "./ft-detached-line.component.html",
  styleUrls: ["./ft-detached-line.component.scss"],
})
export class FtDetachedLineComponent implements OnInit {
  @Input() trace: TraceDto;

  @ViewChild(MatMenuTrigger, null)
  contextMenu: MatMenuTrigger;
  contextMenuPosition = { x: "0px", y: "0px" };

  constructor(
    private router: Router,
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
    console.log(this.trace);
    this.router.navigate(["/trace-information", this.trace.traceId]);
  }
  assignBaseRefs() {
    this.prepareDataForAssignBaseRefs();
    this.router.navigate(["/assign-base", this.trace.traceId]);
  }

  displayState() {
    const dict = {
      type: "traceId",
      traceId: this.trace.traceId,
      fileId: null,
    };
    this.dataStorage.data = dict;
    this.router.navigate(["/trace-state"]);
  }

  displayStatistics() {
    this.router.navigate(["/trace-statistics", this.trace.traceId]);
  }

  prepareDataForAssignBaseRefs() {
    const dict = { trace: this.trace };
    this.dataStorage.data = dict;
  }
}
