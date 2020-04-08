import { Component, OnInit, Input, ViewChild } from "@angular/core";
import { MatMenuTrigger } from "@angular/material";
import { Router } from "@angular/router";
import { FtDetachedTracesProvider } from "src/app/providers/ft-detached-traces";
import { RtuDto } from "src/app/models/dtos/rtuTree/rtuDto";

@Component({
  selector: "ft-free-port",
  templateUrl: "./ft-free-port.component.html",
  styleUrls: ["./ft-free-port.component.scss"],
})
export class FtFreePortComponent implements OnInit {
  @Input() port: number;
  @Input() parentRtu: RtuDto;

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
    this.dataStorage.data = this.parentRtu;
    this.router.navigate(["/port-attach-trace", this.port]);
  }
  attachOpticalSwitch() {
    this.router.navigate(["/port-attach-otau", this.port]);
  }
  measurementClient() {
    this.router.navigate(["/port-measurement-client", this.port]);
  }
}
