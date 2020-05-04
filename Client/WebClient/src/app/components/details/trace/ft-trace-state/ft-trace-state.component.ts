import { Component, OnInit } from "@angular/core";
import { TraceStateDto, Foo } from "src/app/models/dtos/trace/traceStateDto";
import { OneApiService } from "src/app/api/one.service";
import { ActivatedRoute } from "@angular/router";
import { TranslateService } from "@ngx-translate/core";
import { FiberState } from "src/app/models/enums/fiberState";

@Component({
  selector: "ft-trace-state",
  templateUrl: "./ft-trace-state.component.html",
  styleUrls: ["./ft-trace-state.component.css"],
})
export class FtTraceStateComponent implements OnInit {
  vm: TraceStateDto = new TraceStateDto();
  public isAccidentsVisible: boolean;
  public isSpinnerVisible: boolean;

  constructor(
    private activeRoute: ActivatedRoute,
    private oneApiService: OneApiService,
    private ts: TranslateService
  ) {}

  ngOnInit() {
    this.isSpinnerVisible = true;
    const id = this.activeRoute.snapshot.paramMap.get("id");
    this.oneApiService
      .getRequest(`trace/state/${id}`)
      .subscribe((res: TraceStateDto) => {
        console.log(res);
        console.log(res.stateAt);
        this.vm = res;
        this.isAccidentsVisible =
          res.traceState !== FiberState.Ok &&
          res.traceState !== FiberState.NoFiber;
        console.log(this.vm.stateAt);
        const exp = new Foo();
        exp.a = this.vm.registrationTimestamp;
        exp.b = this.vm.sorFileId;
        console.log(exp.c);
        this.isSpinnerVisible = false;
      });
  }
}
