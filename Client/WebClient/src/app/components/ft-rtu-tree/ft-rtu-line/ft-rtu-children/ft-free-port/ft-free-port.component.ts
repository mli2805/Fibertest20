import { Component, OnInit, Input, ViewChild } from "@angular/core";
import { MatMenuTrigger } from "@angular/material";
import { Router } from "@angular/router";
import { FtDetachedTracesProvider } from "src/app/providers/ft-detached-traces-provider";
import { RtuDto } from "src/app/models/dtos/rtuTree/rtuDto";
import { OtauPortDto } from "src/app/models/underlying/otauPortDto";
import { settings } from "cluster";

@Component({
  selector: "ft-free-port",
  templateUrl: "./ft-free-port.component.html",
  styleUrls: ["./ft-free-port.component.scss"],
})
export class FtFreePortComponent implements OnInit {
  @Input() port: number;
  @Input() parentRtu: RtuDto;
  @Input() isPortOnMainCharon: boolean;
  @Input() otauId: string;
  @Input() serial: string;

  @ViewChild(MatMenuTrigger, null)
  contextMenu: MatMenuTrigger;
  contextMenuPosition = { x: "0px", y: "0px" };

  constructor(
    private router: Router,
    private dataStorage: FtDetachedTracesProvider
  ) {}

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
    this.prepareDataForAttachment();
    this.router.navigate(["/port-attach-trace"]);
  }

  attachOpticalSwitch() {
    this.prepareDataForAttachment();
    this.router.navigate(["/port-attach-otau"]);
  }

  measurementClient() {
    this.prepareData();
    this.router.navigate(["/port-measurement-client"]);
  }

  prepareData() {
    const dict = {
      rtuId: this.parentRtu.rtuId,
      otauPortDto: this.compileOtauPortDto(),
    };
    this.dataStorage.data = dict;
  }

  compileOtauPortDto(): OtauPortDto {
    const selectedPort = new OtauPortDto();
    selectedPort.opticalPort = this.port;
    selectedPort.isPortOnMainCharon = this.isPortOnMainCharon;
    selectedPort.otauId = this.otauId;
    selectedPort.serial = this.serial;
    return selectedPort;
  }

  prepareDataForAttachment() {
    this.dataStorage.selectedRtu = this.parentRtu;
    this.dataStorage.selectedPort = this.compileOtauPortDto();
  }
}
