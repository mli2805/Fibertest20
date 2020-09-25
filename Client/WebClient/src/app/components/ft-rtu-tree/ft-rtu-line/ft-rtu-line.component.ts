import { Component, OnInit, Input, ViewChild } from "@angular/core";
import { RtuDto } from "src/app/models/dtos/rtuTree/rtuDto";
import { MatMenuTrigger, MatDialog } from "@angular/material";
import { Router } from "@angular/router";
import { MonitoringMode } from "src/app/models/enums/monitoringMode";
import { RequestAnswer } from "src/app/models/underlying/requestAnswer";
import { ReturnCode } from "src/app/models/enums/returnCode";
import {
  FtRtuTreeEventService,
  RtuTreeEvent,
} from "../ft-rtu-tree-event-service";
import { OneApiService } from "src/app/api/one.service";
import {
  RtuMonitoringSettingsDto,
  RtuMonitoringPortDto,
} from "src/app/models/dtos/rtu/rtuMonitoringSettingsDto";
import { PortMonitoringMode } from "src/app/models/enums/portMonitoringMode";
import {
  FtMessageBox,
  MessageBoxButton,
  MessageBoxStyle,
} from "../../ft-simple-dialog/ft-message-box";
import { TranslateService } from "@ngx-translate/core";

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
    private ftRtuTreeEventService: FtRtuTreeEventService,
    private ts: TranslateService,
    private matDialog: MatDialog
  ) {}

  ngOnInit() {}

  expand(rtu: RtuDto) {
    rtu.expanded = !rtu.expanded;
    this.ftRtuTreeEventService.emitEvent(RtuTreeEvent.saveExpanded);
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

  async automaticMode(rtu: RtuDto) {
    const id = rtu.rtuId;
    console.log("automatic pressed id=", id);
    this.ftRtuTreeEventService.emitEvent(RtuTreeEvent.showSpinner);

    const dto = (await this.oneApiService
      .getRequest(`rtu/monitoring-settings/${id}`)
      .toPromise()) as RtuMonitoringSettingsDto;

    if (
      !dto.lines.some((l) => l.portMonitoringMode === PortMonitoringMode.On)
    ) {
      this.ftRtuTreeEventService.emitEvent(RtuTreeEvent.hideSpinner);

      const answer = await FtMessageBox.show(
        this.matDialog,
        this.ts.instant("SID_No_traces_selected_for_monitoring_"),
        this.ts.instant("SID_Error_"),
        "",
        MessageBoxButton.Ok,
        false,
        MessageBoxStyle.Full,
        "600px"
      ).toPromise();
      console.log(answer);
      return;
    }

    dto.monitoringMode = MonitoringMode.On;
    const res = (await this.oneApiService
      .postRequest(`rtu/monitoring-settings/${id}`, dto)
      .toPromise()) as RequestAnswer;
    console.log(res);
    if (res.returnCode === ReturnCode.MonitoringSettingsAppliedSuccessfully) {
      this.router.navigate(["/ft-main-nav/rtu-tree"]);
    }
  }

  isManualModeDisabled(rtu: RtuDto): boolean {
    return rtu.monitoringMode !== MonitoringMode.On;
  }

  isAutomaticModeDisabled(rtu: RtuDto): boolean {
    return rtu.monitoringMode !== MonitoringMode.Off;
  }
}
