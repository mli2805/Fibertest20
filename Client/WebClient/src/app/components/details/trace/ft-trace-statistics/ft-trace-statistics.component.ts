import { Component, OnInit, ViewChild, AfterViewInit } from "@angular/core";
import { TraceStatisticsDto } from "src/app/models/dtos/trace/traceStatisticsDto";
import { ActivatedRoute, Router } from "@angular/router";
import { MatPaginator, MatMenuTrigger } from "@angular/material";
import { tap } from "rxjs/operators";
import { OneApiService } from "src/app/api/one.service";
import { MeasurementDto } from "src/app/models/dtos/measurementDto";
import { SorFileManager } from "src/app/utils/sorFileManager";
import { FiberState } from "src/app/models/enums/fiberState";
import { BaseRefType } from "src/app/models/enums/baseRefType";
import { TraceStateParams } from "../trace-state-params";

@Component({
  selector: "ft-trace-statistics",
  templateUrl: "./ft-trace-statistics.component.html",
  styleUrls: ["./ft-trace-statistics.component.css"],
})
export class FtTraceStatisticsComponent implements OnInit, AfterViewInit {
  vm: TraceStatisticsDto = new TraceStatisticsDto();
  public fullCount = 345;
  public isNotLoaded = true;

  columnsToDisplayBaseRefs = ["baseRefType", "assignmentTimestamp", "username"];
  columnsToDisplay = [
    "sorFileId",
    "baseRefType",
    "eventRegistrationTimestamp",
    "isEvent",
    "traceState",
  ];

  @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;

  @ViewChild(MatMenuTrigger, null)
  contextMenu: MatMenuTrigger;
  contextMenuPosition = { x: "0px", y: "0px" };

  constructor(
    private router: Router,
    private activeRoute: ActivatedRoute,
    private oneApiService: OneApiService
  ) {}

  async ngOnInit() {
    const id = this.activeRoute.snapshot.paramMap.get("id");
    const params = {
      pageNumber: "0",
      pageSize: "13",
    };

    this.vm = (await this.oneApiService
      .getRequest(`trace/statistics/${id}`, params)
      .toPromise()) as TraceStatisticsDto;

    console.log("trace statistics initial page received");
    this.fullCount = this.vm.measFullCount;
    this.isNotLoaded = false;
  }

  ngAfterViewInit() {
    this.paginator.page
      .pipe(
        tap(() => {
          this.loadPage();
        })
      )
      .subscribe();
  }

  async loadPage() {
    const id = this.activeRoute.snapshot.paramMap.get("id");

    const params = {
      pageNumber: this.paginator.pageIndex.toString(),
      pageSize: this.paginator.pageSize.toString(),
    };

    const res = (await this.oneApiService
      .getRequest(`trace/statistics/${id}`, params)
      .toPromise()) as TraceStatisticsDto;

    console.log("trace statistics page received");
    this.vm = res;
    this.fullCount = res.measFullCount;
    this.isNotLoaded = false;
  }

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
      this.vm.header.traceTitle,
      this.contextMenu.menuData.row.eventRegistrationTimestamp
    );
  }

  async saveRef(isBaseIncluded: boolean) {
    SorFileManager.Download(
      this.oneApiService,
      this.contextMenu.menuData.row.sorFileId,
      isBaseIncluded,
      this.vm.header.traceTitle,
      this.contextMenu.menuData.row.eventRegistrationTimestamp
    );
  }

  showRftsEvents() {
    SorFileManager.ShowRftsEvents(
      this.router,
      this.contextMenu.menuData.row.sorFileId
    );
  }

  showTraceState() {
    this.router.navigate(["/ft-main-nav/trace-state"]);
  }

  getTraceStateColor(traceState: FiberState, baseRefType: BaseRefType) {
    if (traceState === FiberState.Ok) {
      return "white";
    }
    if (baseRefType === BaseRefType.Fast) {
      return "yellow";
    }

    switch (traceState) {
      case FiberState.Suspicion:
        return "yellow";
      case FiberState.Major:
        return "rgb(255, 0, 255)";
      case FiberState.Minor:
        return "rgb(128, 128, 192)";
      case FiberState.Critical:
      case FiberState.FiberBreak:
      case FiberState.NoFiber:
        return "red";
    }
    return "transparent";
  }
}
