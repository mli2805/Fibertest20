import { Component, Input, OnInit, ViewChild } from "@angular/core";
import { MatMenuTrigger } from "@angular/material";
import { Router } from "@angular/router";
import { OneApiService } from "src/app/api/one.service";
import { MeasurementDto } from "src/app/models/dtos/measurementDto";
import { BaseRefInfoDto } from "src/app/models/underlying/baseRefInfoDto";
import { BaseRefPipe } from "src/app/pipes/base-ref-type.pipe";
import { SorFileManager } from "src/app/utils/sorFileManager";

@Component({
  selector: "ft-base-refs",
  templateUrl: "./ft-base-refs.component.html",
  styleUrls: ["./ft-base-refs.component.css"],
  providers: [BaseRefPipe]
})
export class FtBaseRefsComponent implements OnInit {
  @Input() vm: BaseRefInfoDto[] = [];
  @Input() traceTitle = "";

  columnsToDisplayBaseRefs = ["baseRefType", "assignmentTimestamp", "username"];

  @ViewChild(MatMenuTrigger, null)
  contextMenu: MatMenuTrigger;
  contextMenuPosition = { x: "0px", y: "0px" };

  constructor(
    private router: Router,
    private oneApiService: OneApiService,
    private baseRefPipe: BaseRefPipe
  ) {}

  async ngOnInit() {}

  onContextMenu(event: MouseEvent, row: MeasurementDto) {
    event.preventDefault();
    this.contextMenuPosition.x = event.clientX + "px";
    this.contextMenuPosition.y = event.clientY + "px";
    this.contextMenu.menuData = { row };
    this.contextMenu.openMenu();
    this.contextMenu.focus("mouse");
  }

  async showRef(isBaseIncluded: boolean) {
    SorFileManager.Show(
      this.router,
      true,
      this.contextMenu.menuData.row.sorFileId,
      "",
      isBaseIncluded,
      this.traceTitle,
      this.contextMenu.menuData.row.eventRegistrationTimestamp,
      ""
    );
  }

  async saveRef(isBaseIncluded: boolean) {
    SorFileManager.Download(
      this.oneApiService,
      this.contextMenu.menuData.row.sorFileId,
      this.baseRefPipe.transform(this.contextMenu.menuData.row.baseRefType),
      isBaseIncluded,
      this.traceTitle,
      this.contextMenu.menuData.row.assignmentTimestamp,
      ""
    );
  }
}
