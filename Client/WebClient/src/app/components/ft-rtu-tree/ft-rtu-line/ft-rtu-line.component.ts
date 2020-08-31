import { Component, OnInit, Input, ViewChild } from "@angular/core";
import { RtuDto } from "src/app/models/dtos/rtuTree/rtuDto";
import { MatMenuTrigger } from "@angular/material";
import { Router } from "@angular/router";
import { MonitoringMode } from "src/app/models/enums/monitoringMode";
import { RequestAnswer } from "src/app/models/underlying/requestAnswer";
import { ReturnCode } from "src/app/models/enums/returnCode";
import {
  FtRtuTreeEventService,
  RtuTreeEvent,
} from "../ft-rtu-tree-event-service";
import { OneApiService } from "src/app/api/one.service";

@Component({
  selector: "ft-rtu-line",
  templateUrl: "./ft-rtu-line.component.html",
  styleUrls: ["./ft-rtu-line.component.css"],
})
export class FtRtuLineComponent implements OnInit {
  @Input() rtu: RtuDto;

  @ViewChild(MatMenuTrigger, null)
  contextMenu: MatMenuTrigger;
  contextMenuPosition = { x: "0px", y: "0px" };

  constructor(
    private router: Router,
    private oneApiService: OneApiService,
    private ftRtuTreeEventService: FtRtuTreeEventService
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
    this.router.navigate(["/ft-main-nav/rtu-information", rtu.rtuId]);
  }

  networkSettings(rtu: RtuDto) {
    this.router.navigate(["/ft-main-nav/rtu-network-settings", rtu.rtuId]);
  }

  state(rtu: RtuDto) {
    this.router.navigate(["/ft-main-nav/rtu-state", rtu.rtuId]);
  }

  monitoringSettings(rtu: RtuDto) {
    this.router.navigate(["/ft-main-nav/rtu-monitoring-settings", rtu.rtuId]);
  }

  manualMode(rtu: RtuDto) {
    this.ftRtuTreeEventService.emitEvent(RtuTreeEvent.showSpinner);
    const id = rtu.rtuId;
    console.log("manual pressed id=", id);
    this.oneApiService
      .postRequest(`rtu/stop-monitoring/${id}`, rtu.rtuMaker)
      .subscribe((res: boolean) => {
        this.ftRtuTreeEventService.emitEvent(RtuTreeEvent.fetchTree);
        console.log(res);
        if (res === true) {
          this.router.navigate(["/ft-main-nav/rtu-tree"]);
        }
      });
  }

  automaticMode(rtu: RtuDto) {
    this.ftRtuTreeEventService.emitEvent(RtuTreeEvent.showSpinner);
    const id = rtu.rtuId;
    console.log("automatic pressed id=", id);
    this.oneApiService
      .postRequest(`rtu/start-monitoring/${id}`, null)
      .subscribe((res: RequestAnswer) => {
        this.ftRtuTreeEventService.emitEvent(RtuTreeEvent.fetchTree);
        console.log(res);
        if (
          res.returnCode === ReturnCode.MonitoringSettingsAppliedSuccessfully
        ) {
          this.router.navigate(["/ft-main-nav/rtu-tree"]);
        }
      });
  }

  isManualModeDisabled(rtu: RtuDto): boolean {
    return rtu.monitoringMode !== MonitoringMode.On;
  }

  isAutomaticModeDisabled(rtu: RtuDto): boolean {
    return rtu.monitoringMode !== MonitoringMode.Off;
  }
}
