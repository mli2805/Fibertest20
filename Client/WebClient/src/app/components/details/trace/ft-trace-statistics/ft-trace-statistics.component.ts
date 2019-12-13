import { Component, OnInit, Input } from "@angular/core";
import { TraceStatisticsDto } from "src/app/models/dtos/traceStatisticsDto";
import { ActivatedRoute } from "@angular/router";
import { TraceApiService } from "src/app/api/trace.service";

@Component({
  selector: "ft-trace-statistics",
  templateUrl: "./ft-trace-statistics.component.html",
  styleUrls: ["./ft-trace-statistics.component.css"]
})
export class FtTraceStatisticsComponent implements OnInit {
  vm: TraceStatisticsDto;

  columnsToDisplayBaseRefs = ["baseRefType", "assignmentTimestamp", "username"];
  columnsToDisplay = [
    "sorFileId",
    "baseRefType",
    "eventRegistrationTimestamp",
    "isEvent",
    "traceState"
  ];

  constructor(
    private activeRoute: ActivatedRoute,
    private traceApiService: TraceApiService
  ) {}

  ngOnInit() {
    const id = this.activeRoute.snapshot.paramMap.get("id");
    this.traceApiService
      .getOneTrace(id, "statistics")
      .subscribe((res: TraceStatisticsDto) => {
        console.log("trace statistics received");
        this.vm = res;
      });
  }
}
