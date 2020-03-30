import { Component, OnInit, Input, ViewChild } from "@angular/core";
import { RtuDto } from "src/app/models/dtos/rtuTree/rtuDto";
import { MatMenuTrigger } from "@angular/material";
import { Router } from "@angular/router";
import { MonitoringMode } from "src/app/models/enums/monitoringMode";

@Component({
  selector: "ft-rtu-line",
  templateUrl: "./ft-rtu-line.component.html",
  styleUrls: ["./ft-rtu-line.component.css"]
})
export class FtRtuLineComponent implements OnInit {
  @Input() rtu: RtuDto;

  @ViewChild(MatMenuTrigger, null)
  contextMenu: MatMenuTrigger;
  contextMenuPosition = { x: "0px", y: "0px" };

  constructor(private router: Router) {}

  ngOnInit() {}

  expand(rtu: RtuDto) {
    rtu.expanded = !rtu.expanded;
  }

  onContextMenu(event: MouseEvent) {
    event.preventDefault();
    this.contextMenuPosition.x = event.clientX + "px";
    this.contextMenuPosition.y = event.clientY + "px";
    this.contextMenu.menuData = { item: this.rtu.title };
    this.contextMenu.openMenu();
    this.contextMenu.focus("mouse");
  }

  information(rtu: RtuDto) {
    this.router.navigate(["/rtu-information", rtu.rtuId]);
  }

  networkSettings(rtu: RtuDto) {
    this.router.navigate(["/rtu-network-settings", rtu.rtuId]);
  }

  state(rtu: RtuDto) {
    this.router.navigate(["/rtu-state", rtu.rtuId]);
  }

  monitoringSettings(rtu: RtuDto) {
    this.router.navigate(["/rtu-monitoring-settings", rtu.rtuId]);
  }

  manualMode(rtu: RtuDto) {}
  automaticMode(rtu: RtuDto) {}

  isManualModeDisabled(rtu: RtuDto): boolean {
    console.log("r");
    return rtu.monitoringMode === MonitoringMode.Off;
  }
}
