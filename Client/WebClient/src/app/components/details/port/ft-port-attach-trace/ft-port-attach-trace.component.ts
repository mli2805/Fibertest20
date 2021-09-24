import { Component, OnInit, Input } from "@angular/core";
import { TraceDto } from "src/app/models/dtos/rtuTree/traceDto";
import { RtuDto } from "src/app/models/dtos/rtuTree/rtuDto";
import { ChildDto } from "src/app/models/dtos/rtuTree/childDto";
import { Router } from "@angular/router";
import { AttachTraceDto } from "src/app/models/dtos/port/attachTraceDto";
import { OneApiService } from "src/app/api/one.service";
import { RequestAnswer } from "src/app/models/underlying/requestAnswer";
import { ReturnCode } from "src/app/models/enums/returnCode";
import { ReturnCodePipe } from "src/app/pipes/return-code.pipe";

@Component({
  selector: "ft-port-attach-trace",
  templateUrl: "./ft-port-attach-trace.component.html",
  styleUrls: ["./ft-port-attach-trace.component.css"],
})
export class FtPortAttachTraceComponent implements OnInit {
  traceList: TraceDto[];
  selectedTrace: string;
  rtu: RtuDto;
  public isSpinnerVisible = false;
  public isButtonDisabled = false;
  public resultMessage: string;

  constructor(
    private router: Router,
    private oneApiService: OneApiService,
    private returnCodePipe: ReturnCodePipe
  ) {}

  ngOnInit() {
    const params = JSON.parse(sessionStorage.getItem("attachTraceParams"));
    this.rtu = params.selectedRtu;
    this.traceList = this.rtu.children
      .filter((c) => c.childType === 1 && c.port === -1)
      .map((ch: ChildDto) => ch as TraceDto);
    if (this.traceList.length > 0) {
      this.selectedTrace = this.traceList[0].traceId;
    }
  }

  async attachTrace() {
    this.isSpinnerVisible = true;
    this.isButtonDisabled = true;
    const trace: TraceDto = this.traceList.find(
      (t) => t.traceId === this.selectedTrace
    );
    const params = JSON.parse(sessionStorage.getItem("attachTraceParams"));
    const cmd = new AttachTraceDto();
    cmd.TraceId = trace.traceId;
    cmd.RtuMaker = this.rtu.rtuMaker;
    cmd.OtauPortDto = params.selectedPort;
    console.log(cmd);
    const res = (await this.oneApiService
      .postRequest("port/attach-trace", cmd)
      .toPromise()) as RequestAnswer;
    this.isSpinnerVisible = false;
    this.isButtonDisabled = false;
    console.log(res);
    if (res.returnCode === ReturnCode.Ok) {
      this.router.navigate(["/ft-main-nav/rtu-tree"]);
    } else {
      if (res.errorMessage !== null) {
        this.resultMessage = res.errorMessage;
      } else {
        this.resultMessage = this.returnCodePipe.transform(res.returnCode);
      }
    }
  }

  cancel() {
    this.router.navigate(["/ft-main-nav/rtu-tree"]);
  }
}
