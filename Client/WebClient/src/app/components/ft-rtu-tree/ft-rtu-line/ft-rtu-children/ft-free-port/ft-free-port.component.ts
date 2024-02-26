import { Component, OnInit, Input, ViewChild } from "@angular/core";
import { MatMenuTrigger } from "@angular/material";
import { Router } from "@angular/router";
import { RtuDto } from "src/app/models/dtos/rtuTree/rtuDto";
import { RegistrationAnswerDto } from "src/app/models/dtos/registrationAnswerDto";
import { MonitoringMode } from "src/app/models/enums/monitoringMode";
import { Role } from "src/app/models/enums/role";
import { OtauPortDto } from "src/app/models/underlying/otauPortDto";
import { RtuPartState } from "src/app/models/enums/rtuPartState";

@Component({
  selector: "ft-free-port",
  templateUrl: "./ft-free-port.component.html",
  styleUrls: ["./ft-free-port.component.scss"],
})
export class FtFreePortComponent implements OnInit {
  @Input() port: number;
  @Input() masterPort: number;
  @Input() parentRtu: RtuDto;
  @Input() isPortOnMainCharon: boolean;
  @Input() otauId: string;
  @Input() serial: string;

  @ViewChild(MatMenuTrigger, null)
  contextMenu: MatMenuTrigger;
  contextMenuPosition = { x: "0px", y: "0px" };

  user: RegistrationAnswerDto;

  constructor(private router: Router) {}

  ngOnInit() {
    this.user = JSON.parse(sessionStorage.getItem("currentUser"));
  }

  onContextMenu(event: MouseEvent) {
    event.preventDefault();
    this.contextMenuPosition.x = event.clientX + "px";
    this.contextMenuPosition.y = event.clientY + "px";
    this.contextMenu.menuData = { item: this.port };
    this.contextMenu.openMenu();
    this.contextMenu.focus("mouse");
  }

  attachTraceFromList() {
    this.prepareCallerInfo("attachTraceParams");
    this.router.navigate(["/ft-main-nav/port-attach-trace"]);
  }

  attachOpticalSwitch() {
    this.prepareCallerInfo("attachOtauParams");
    this.router.navigate(["/ft-main-nav/port-attach-otau"]);
  }

  measurementClient() {
    this.prepareCallerInfo("measurementClientParams");
    this.router.navigate(["/ft-main-nav/port-measurement-client"]);
  }

  prepareCallerInfo(paramName: string) {
    const dict = {
      selectedRtu: this.parentRtu,
      selectedPort: this.compileOtauPortDto(),
    };
    sessionStorage.setItem(paramName, JSON.stringify(dict));
  }

  // selectedPort - порт к которому идет подключение или измерение
  // и если он не на главном переключателе, то надо прислать
  // mainOtauPort - порт в который надо переключить главный переключатель (к которому подключен этот боп)
  // а если порт на главном переключателе, то данные в selectedPort а mainOtauPort не надо заполнять
  compileOtauPortDto(): OtauPortDto {
    const selectedPort = new OtauPortDto();
    selectedPort.opticalPort = this.port;
    selectedPort.isPortOnMainCharon = this.isPortOnMainCharon;
    selectedPort.otauId = this.otauId;
    selectedPort.serial = this.serial;
    selectedPort.mainCharonPort = this.masterPort;
    return selectedPort;
  }

  public isAttachTraceDisabled() {
    return this.user.role > Role.WebOperator || this.isRtuAvailable() !== true;
  }

  public isAttachSwitchDisabled() {
    return (
      this.parentRtu.monitoringMode === MonitoringMode.On ||
      this.isRtuAvailable() !== true ||
      this.user.role > Role.Root
    );
  }

  public isMeasurementClientDisabled() {
     return this.user.role > Role.WebOperator || this.isRtuAvailable() !== true;
  }

  private isRtuAvailable(): boolean {
    return (
      this.parentRtu.mainChannelState === RtuPartState.Ok ||
      this.parentRtu.reserveChannelState === RtuPartState.Ok
    );
  }
}
