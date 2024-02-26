import { Component, OnInit, Input, ViewChild } from "@angular/core";
import { RtuDto } from "src/app/models/dtos/rtuTree/rtuDto";
import { MatMenuTrigger, MatDialog } from "@angular/material";
import { Router } from "@angular/router";
import { MonitoringMode } from "src/app/models/enums/monitoringMode";
import { RequestAnswer } from "src/app/models/underlying/requestAnswer";
import {
  FtRtuTreeEventService,
  RtuTreeEvent,
} from "../ft-rtu-tree-event-service";
import { OneApiService } from "src/app/api/one.service";
import { RtuMonitoringSettingsDto } from "src/app/models/dtos/rtu/rtuMonitoringSettingsDto";
import { PortMonitoringMode } from "src/app/models/enums/portMonitoringMode";
import {
  FtMessageBox,
  MessageBoxButton,
  MessageBoxStyle,
} from "../../ft-simple-dialog/ft-message-box";
import { TranslateService } from "@ngx-translate/core";
import { RtuPartState } from "src/app/models/enums/rtuPartState";
import { RegistrationAnswerDto } from "src/app/models/dtos/registrationAnswerDto";
import { Role } from "src/app/models/enums/role";
import { StopMonitoringDto } from "src/app/models/dtos/rtu/monitoringStoppedDto";

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

  user: RegistrationAnswerDto;

  constructor(
    private router: Router,
    private oneApiService: OneApiService,
    private ftRtuTreeEventService: FtRtuTreeEventService,
    private ts: TranslateService,
    private matDialog: MatDialog
  ) {}

  ngOnInit() {
    this.user = JSON.parse(
      sessionStorage.getItem("currentUser")
    );
  }

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

  async manualMode(rtu: RtuDto) {
    this.ftRtuTreeEventService.emitEvent(RtuTreeEvent.showSpinner);
    const dto = new StopMonitoringDto();
    dto.connectionId = this.user.connectionId;
    dto.rtuId = rtu.rtuId;
    dto.rtuMaker = rtu.rtuMaker;
    console.log(dto);
    const res = (await this.oneApiService
      .postRequest(`rtu/stop-monitoring`, dto)
      .toPromise()) as boolean;
    console.log("result: ", res);
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

      await FtMessageBox.showAndGoAlong(
        this.matDialog,
        this.ts.instant("SID_No_traces_selected_for_monitoring_"),
        this.ts.instant("SID_Error_"),
        "",
        MessageBoxButton.Ok,
        false,
        MessageBoxStyle.Full,
        "600px"
      );
      return;
    }

    dto.connectionId = this.user.connectionId;
    dto.monitoringMode = MonitoringMode.On;
    const res = (await this.oneApiService
      .postRequest(`rtu/monitoring-settings/${id}`, dto)
      .toPromise()) as RequestAnswer;
    console.log(res);
  }

  isManualModeDisabled(rtu: RtuDto): boolean {
    return this.user.role > Role.WebOperator ||
            rtu.monitoringMode !== MonitoringMode.On
            || !this.isRtuAvailable();
  }

  isAutomaticModeDisabled(rtu: RtuDto): boolean {
    return this.user.role > Role.WebOperator ||
            rtu.monitoringMode !== MonitoringMode.Off
            || !this.isRtuAvailable();
  }

  isRtuAvailable() : boolean {
    return this.rtu.mainChannelState === RtuPartState.Ok || this.rtu.reserveChannelState === RtuPartState.Ok;
  }
}
