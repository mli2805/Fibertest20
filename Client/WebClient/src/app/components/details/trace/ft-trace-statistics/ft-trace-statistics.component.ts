import { Component, OnInit, ViewChild, AfterViewInit } from "@angular/core";
import { TraceStatisticsDto } from "src/app/models/dtos/trace/traceStatisticsDto";
import { ActivatedRoute } from "@angular/router";
import { MatPaginator } from "@angular/material";
import { tap } from "rxjs/operators";
import { OneApiService } from "src/app/api/one.service";

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

  constructor(
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
}
