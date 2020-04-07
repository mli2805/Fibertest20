import { Component, OnInit, Input, ViewChild } from "@angular/core";
import { MatMenuTrigger } from "@angular/material";
import { Router } from "@angular/router";

@Component({
  selector: "ft-free-port",
  templateUrl: "./ft-free-port.component.html",
  styleUrls: ["./ft-free-port.component.scss"],
})
export class FtFreePortComponent implements OnInit {
  @Input() port: number;

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
    this.router.navigate(["/port-attach-trace", this.port]);
  }
  attachOpticalSwitch() {
    this.router.navigate(["/port-attach-otau", this.port]);
  }
  measurementClient() {
    this.router.navigate(["/port-measurement-client", this.port]);
  }
}
