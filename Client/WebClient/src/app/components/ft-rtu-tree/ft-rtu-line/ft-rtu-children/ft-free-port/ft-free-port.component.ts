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

  constructor(private router: Router) {}

  ngOnInit() {}

  onContextMenu(event: MouseEvent) {
    event.preventDefault();
    this.contextMenuPosition.x = event.clientX + "px";
    this.contextMenuPosition.y = event.clientY + "px";
    this.contextMenu.menuData = { item: this.port };
    this.contextMenu.openMenu();
    this.contextMenu.focus("mouse");
  }

  attachTraceFromList() {
    this.prepareDataForAttachment("attachTraceParams");
    this.router.navigate(["/ft-main-nav/port-attach-trace"]);
  }

  attachOpticalSwitch() {
    this.prepareDataForAttachment("attachOtauParams");
    this.router.navigate(["/ft-main-nav/port-attach-otau"]);
  }

  measurementClient() {
    this.prepareDataForMeasurement();
    this.router.navigate(["/ft-main-nav/port-measurement-client"]);
  }

  prepareDataForMeasurement() {
    const dict = {
      rtuId: this.parentRtu.rtuId,
      otauPortDto: this.compileOtauPortDto(),
    };
    sessionStorage.setItem("measurementClientParams", JSON.stringify(dict));
  }

  prepareDataForAttachment(paramName: string) {
    const dict = {
      selectedRtu: this.parentRtu,
      selectedPort: this.compileOtauPortDto(),
    };
    sessionStorage.setItem(paramName, JSON.stringify(dict));
  }

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
    const user: RegistrationAnswerDto = JSON.parse(
      sessionStorage.getItem("currentUser")
    );
    return user.role > Role.WebOperator || this.isRtuAvailable() !== true;
  }

  public isAttachSwitchDisabled() {
    const user: RegistrationAnswerDto = JSON.parse(
      sessionStorage.getItem("currentUser")
    );
    return (
      this.parentRtu.monitoringMode === MonitoringMode.On ||
      this.isRtuAvailable() !== true ||
      user.role > Role.Root
    );
  }

  public isMeasurementClientDisabled() {
    const user: RegistrationAnswerDto = JSON.parse(
      sessionStorage.getItem("currentUser")
    );
    return user.role > Role.WebOperator || this.isRtuAvailable() !== true;
  }

  private isRtuAvailable(): boolean {
    return (
      this.parentRtu.mainChannelState === RtuPartState.Ok ||
      this.parentRtu.reserveChannelState === RtuPartState.Ok
    );
  }
}
