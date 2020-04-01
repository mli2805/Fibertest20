import { Component, OnInit, Input, ViewChild } from "@angular/core";
import { OtauWebDto } from "src/app/models/dtos/rtuTree/otauWebDto";
import { MatMenuTrigger } from "@angular/material";
import { RtuDto } from "src/app/models/dtos/rtuTree/rtuDto";
import { RtuApiService } from "src/app/api/rtu.service";
import { DetachOtauDto } from "src/app/models/dtos/rtu/detachOtauDto";

@Component({
  selector: "ft-otau",
  templateUrl: "./ft-otau.component.html",
  styleUrls: ["./ft-otau.component.css"]
})
export class FtOtauComponent implements OnInit {
  @Input() parentRtu: RtuDto;
  @Input() otau: OtauWebDto;

  @ViewChild(MatMenuTrigger, null)
  contextMenu: MatMenuTrigger;
  contextMenuPosition = { x: "0px", y: "0px" };

  constructor(private rtuApiService: RtuApiService) {}

  ngOnInit() {}

  expand() {
    console.log("expand otau clicked");
    this.otau.expanded = !this.otau.expanded;
    console.log("rtu: ", this.parentRtu);
    console.log("otau: ", this.otau);
  }

  onContextMenu(event: MouseEvent) {
    event.preventDefault();
    this.contextMenuPosition.x = event.clientX + "px";
    this.contextMenuPosition.y = event.clientY + "px";
    this.contextMenu.menuData = { item: this.otau.otauId };
    this.contextMenu.openMenu();
    this.contextMenu.focus("mouse");
  }

  removeOtau() {
    console.log("remove otau pressed");
    const detachOtauDto = new DetachOtauDto();
    detachOtauDto.rtuId = this.otau.rtuId;
    detachOtauDto.otauId = this.otau.otauId;
    detachOtauDto.otauAddresses = this.otau.otauNetAddress;
    detachOtauDto.opticalPort = this.otau.port;
    this.rtuApiService
      .postOneRtu(this.parentRtu.rtuId, "detach-otau", detachOtauDto)
      .subscribe((res: any) => {
        console.log(res);
      });
  }

  isRemoveOtauDisabled(): boolean {
    return false;
  }
}
