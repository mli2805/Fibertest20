import { Component, OnInit, ViewChild, AfterViewInit } from "@angular/core";
import { TraceStatisticsDto } from "src/app/models/dtos/trace/traceStatisticsDto";
import { ActivatedRoute } from "@angular/router";
import { TraceApiService } from "src/app/api/trace.service";
import { MatPaginator } from "@angular/material";
import { tap } from "rxjs/operators";

@Component({
  selector: "ft-trace-statistics",
  templateUrl: "./ft-trace-statistics.component.html",
  styleUrls: ["./ft-trace-statistics.component.css"]
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
    "traceState"
  ];

  @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;

  constructor(
    private activeRoute: ActivatedRoute,
    private traceApiService: TraceApiService
  ) {}

  ngOnInit() {
    const id = this.activeRoute.snapshot.paramMap.get("id");
    this.traceApiService
      .getOneTrace("statistics", id, 0, 8)
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
    this.traceApiService
      .getOneTrace(
        "statistics",
        id,
        this.paginator.pageIndex,
        this.paginator.pageSize
      )
      .subscribe((res: TraceStatisticsDto) => {
        console.log("trace statistics page received");
        this.vm = res;
        this.fullCount = res.measFullCount;
        this.isNotLoaded = false;
      });
  }
}
