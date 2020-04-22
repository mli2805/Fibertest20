import { Component, OnInit, Input } from "@angular/core";
import { TraceInformationDto } from "src/app/models/dtos/trace/traceInformationDto";
import { ActivatedRoute } from "@angular/router";
import { TranslateService } from "@ngx-translate/core";
import { OneApiService } from "src/app/api/one.service";

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
    private oneApiService: OneApiService,
    private ts: TranslateService
  ) {}

  ngOnInit() {
    this.isSpinnerVisible = true;
    const id = this.activeRoute.snapshot.paramMap.get("id");
    console.log(id);
    this.oneApiService
      .getRequest(`trace/information/${id}`)
      .subscribe((res: TraceInformationDto) => {
        this.isSpinnerVisible = false;
        console.log(res);
        this.vm = res;
        this.vm.port =
          res.port === "-1" ? this.ts.instant("SID_not_attached") : res.port;
      });
  }
}
