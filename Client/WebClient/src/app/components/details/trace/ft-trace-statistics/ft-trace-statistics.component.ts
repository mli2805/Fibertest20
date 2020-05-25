import { Component, OnInit, ViewChild, AfterViewInit } from "@angular/core";
import { TraceStatisticsDto } from "src/app/models/dtos/trace/traceStatisticsDto";
import { ActivatedRoute, Router } from "@angular/router";
import { MatPaginator, MatMenuTrigger } from "@angular/material";
import { tap } from "rxjs/operators";
import { OneApiService } from "src/app/api/one.service";
import { MeasurementDto } from "src/app/models/dtos/measurementDto";
import { ReturnCode } from "src/app/models/enums/returnCode";

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

  ngOnInit() {
    const id = this.activeRoute.snapshot.paramMap.get("id");
    const params = {
      pageNumber: "0",
      pageSize: "13",
    };

    this.oneApiService
      .getRequest(`trace/statistics/${id}`, params)
      .subscribe((res: TraceStatisticsDto) => {
        console.log("trace statistics initial page received");
        this.vm = res;
        this.fullCount = res.measFullCount;
        this.isNotLoaded = false;
      });
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

  loadPage() {
    const id = this.activeRoute.snapshot.paramMap.get("id");

    const params = {
      pageNumber: this.paginator.pageIndex.toString(),
      pageSize: this.paginator.pageSize.toString(),
    };

    this.oneApiService
      .getRequest(`trace/statistics/${id}`, params)
      .subscribe((res: TraceStatisticsDto) => {
        console.log("trace statistics page received");
        this.vm = res;
        this.fullCount = res.measFullCount;
        this.isNotLoaded = false;
      });
  }

  onContextMenu(event: MouseEvent, row: MeasurementDto) {
    event.preventDefault();
    this.contextMenuPosition.x = event.clientX + "px";
    this.contextMenuPosition.y = event.clientY + "px";
    this.contextMenu.menuData = { row };
    this.contextMenu.openMenu();
    this.contextMenu.focus("mouse");
  }

  showRef(param: number) {
    const sorFileId = this.contextMenu.menuData.row.sorFileId;
    if (param === 1) {
      console.log("show ref: ", sorFileId);
    } else {
      console.log("show ref and base: ", sorFileId);
    }

    this.oneApiService
      .getRequest(`misc/get-sor-file/${sorFileId}`)
      .subscribe((res: any) => {
        if (res.ReturnCode === ReturnCode.Error) {
          alert(`failed to fetch sor file ${sorFileId}!`);
          return;
        }
        console.log(`received ${res.SorBytes.length} bytes`);
      });
  }

  saveRef(param: number) {
    if (param === 1) {
      console.log("save ref: ", this.contextMenu.menuData.row.sorFileId);
    } else {
      console.log(
        "save ref and base: ",
        this.contextMenu.menuData.row.sorFileId
      );
    }
  }

  showRftsEvents() {
    console.log("show rfts events: ", this.contextMenu.menuData.row.sorFileId);
  }
  showTraceState() {
    console.log("show trace state: ", this.contextMenu.menuData.row.sorFileId);
    const dict = {
      type: "fileId",
      traceId: null,
      fileId: this.contextMenu.menuData.row.sorFileId,
    };
    sessionStorage.setItem("traceStateParams", JSON.stringify(dict));
    this.router.navigate(["/trace-state"]);
  }
}
