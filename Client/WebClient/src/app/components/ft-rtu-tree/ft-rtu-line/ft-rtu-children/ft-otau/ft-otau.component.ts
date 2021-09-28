import { Component, OnInit, Input, ViewChild } from "@angular/core";
import { OtauWebDto } from "src/app/models/dtos/rtuTree/otauWebDto";
import { MatMenuTrigger } from "@angular/material";
import { RtuDto } from "src/app/models/dtos/rtuTree/rtuDto";
import { DetachOtauDto } from "src/app/models/dtos/rtu/detachOtauDto";
import {
  FtRtuTreeEventService,
  RtuTreeEvent,
} from "../../../ft-rtu-tree-event-service";
import { OneApiService } from "src/app/api/one.service";
import { ReturnCode } from "src/app/models/enums/returnCode";
import { RegistrationAnswerDto } from "src/app/models/dtos/registrationAnswerDto";
import { Role } from "src/app/models/enums/role";
import { MonitoringMode } from "src/app/models/enums/monitoringMode";
import { RequestAnswer } from "src/app/models/underlying/requestAnswer";

@Component({
  selector: "ft-otau",
  templateUrl: "./ft-otau.component.html",
  styleUrls: ["./ft-otau.component.css"],
})
export class FtOtauComponent implements OnInit {
  @Input() parentRtu: RtuDto;
  @Input() otau: OtauWebDto;

  @ViewChild(MatMenuTrigger, null)
  contextMenu: MatMenuTrigger;
  contextMenuPosition = { x: "0px", y: "0px" };

  constructor(
    private oneApiService: OneApiService,
    private ftRtuTreeEventService: FtRtuTreeEventService
  ) {}

  ngOnInit() {}

  expand() {
    this.otau.expanded = !this.otau.expanded;
    this.ftRtuTreeEventService.emitEvent(RtuTreeEvent.saveExpanded);
  }

  onContextMenu(event: MouseEvent) {
    event.preventDefault();
    this.contextMenuPosition.x = event.clientX + "px";
    this.contextMenuPosition.y = event.clientY + "px";
    this.contextMenu.menuData = { item: this.otau.otauId };
    this.contextMenu.openMenu();
    this.contextMenu.focus("mouse");
  }

  async removeOtau() {
    this.ftRtuTreeEventService.emitEvent(RtuTreeEvent.showSpinner);
    const detachOtauDto = new DetachOtauDto();
    detachOtauDto.rtuId = this.otau.rtuId;
    detachOtauDto.rtuMaker = this.parentRtu.rtuMaker;
    detachOtauDto.otauId = this.otau.otauId;
    detachOtauDto.netAddress = this.otau.otauNetAddress;
    detachOtauDto.opticalPort = this.otau.port;
    console.log(detachOtauDto);
    const res = await this.oneApiService
      .postRequest("port/detach-otau", detachOtauDto)
      .toPromise() as RequestAnswer;
    console.log(res);
    if (res.returnCode !== ReturnCode.OtauDetachedSuccesfully) {
      alert("Error");
    }
  }

  isRemoveOtauDisabled(): boolean {
    const user: RegistrationAnswerDto = JSON.parse(sessionStorage.getItem("currentUser"));
    return (
      user.role > Role.Root ||
      this.parentRtu.monitoringMode === MonitoringMode.On
    );
  }
}
