import { Component, OnInit, Input, ViewChild } from "@angular/core";
import { RtuDto } from "src/app/models/dtos/rtuTree/rtuDto";
import { MatMenuTrigger } from "@angular/material";
import { Router, ActivatedRoute } from "@angular/router";
import { MonitoringMode } from "src/app/models/enums/monitoringMode";
import { RtuApiService } from "src/app/api/rtu.service";

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

  constructor(
    private router: Router,
    private activeRoute: ActivatedRoute,
    private rtuApiService: RtuApiService
  ) {}

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

  manualMode(rtu: RtuDto) {
    console.log("manual pressed");
    const id = this.activeRoute.snapshot.paramMap.get("id");
    this.rtuApiService
      .postOneRtu(id, "stop-monitoring", null)
      .subscribe((res: any) => {
        console.log(res);
      });
  }
  automaticMode(rtu: RtuDto) {
    console.log("automatic pressed");
    const id = this.activeRoute.snapshot.paramMap.get("id");
    this.rtuApiService
      .postOneRtu(id, "start-monitoring", null)
      .subscribe((res: any) => {
        console.log(res);
      });
  }

  isManualModeDisabled(rtu: RtuDto): boolean {
    return rtu.monitoringMode !== MonitoringMode.On;
  }

  isAutomaticModeDisabled(rtu: RtuDto): boolean {
    return rtu.monitoringMode !== MonitoringMode.Off;
  }
}
