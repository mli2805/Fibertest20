import { Component, OnInit, Input } from "@angular/core";
import { TraceInformationDto } from "src/app/models/dtos/trace/traceInformationDto";
import { ActivatedRoute } from "@angular/router";
import { TraceApiService } from "src/app/api/trace.service";

@Component({
  selector: "ft-trace-information",
  templateUrl: "./ft-trace-information.component.html",
  styleUrls: ["./ft-trace-information.component.css"],
})
export class FtTraceInformationComponent implements OnInit {
  @Input() vm = new TraceInformationDto();
  public isSpinnerVisible: boolean;

  constructor(
    private activeRoute: ActivatedRoute,
    private traceApiService: TraceApiService
  ) {}

  ngOnInit() {
    this.isSpinnerVisible = true;
    const id = this.activeRoute.snapshot.paramMap.get("id");
    this.traceApiService
      .getOneTrace("information", id, 0, 0)
      .subscribe((res: TraceInformationDto) => {
        this.isSpinnerVisible = false;
        console.log(res);
        this.vm = res;
      });
  }
}
