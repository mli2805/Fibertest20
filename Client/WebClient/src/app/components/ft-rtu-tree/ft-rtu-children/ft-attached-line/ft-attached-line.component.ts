import { Component, OnInit, Input, Output, ViewChild } from "@angular/core";
import { TraceDto } from "src/app/models/dtos/traceDto";
import { TraceStatisticsDto } from "src/app/models/dtos/traceStatisticsDto";
import { MatMenuTrigger, MatMenu } from "@angular/material";

@Component({
  selector: "ft-attached-line",
  templateUrl: "./ft-attached-line.component.html",
  styleUrls: ["./ft-attached-line.component.scss"]
})
export class FtAttachedLineComponent implements OnInit {
  @Input() trace: TraceDto;

  @ViewChild(MatMenuTrigger, null)
  contextMenu: MatMenuTrigger;
  contextMenuPosition = { x: "0px", y: "0px" };

  constructor() {}

  ngOnInit() {}

  onContextMenu(event: MouseEvent) {
    event.preventDefault();
    this.contextMenuPosition.x = event.clientX + "px";
    this.contextMenuPosition.y = event.clientY + "px";
    this.contextMenu.menuData = { item: this.trace.title };
    this.contextMenu.openMenu();
    this.contextMenu.focus("mouse");
  }
}
