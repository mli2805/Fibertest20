import { Component, OnInit, Input } from "@angular/core";
import { TraceDto } from "src/app/models/dtos/rtuTree/traceDto";
import { FtDetachedTracesProvider } from "src/app/providers/ft-detached-traces-provider";
import { RtuDto } from "src/app/models/dtos/rtuTree/rtuDto";
import { ChildDto } from "src/app/models/dtos/rtuTree/childDto";
import { PortApiService } from "src/app/api/port.service";
import { Router } from "@angular/router";
import { AttachTraceDto } from "src/app/models/dtos/port/attachTraceDto";

@Component({
  selector: "ft-port-attach-trace",
  templateUrl: "./ft-port-attach-trace.component.html",
  styleUrls: ["./ft-port-attach-trace.component.css"],
})
export class FtPortAttachTraceComponent implements OnInit {
  traceList: TraceDto[];
  selectedTrace;
  public isSpinnerVisible = false;
  public isButtonDisabled = false;
  public resultMessage: string;

  constructor(
    private router: Router,
    private dataStorage: FtDetachedTracesProvider,
    private portApiService: PortApiService
  ) {}

  ngOnInit() {
    const rtu: RtuDto = this.dataStorage.selectedRtu;
    this.traceList = rtu.children
      .filter((c) => c.childType === 1 && c.port === -1)
      .map((ch: ChildDto) => ch as TraceDto);
    if (this.traceList.length > 0) {
      this.selectedTrace = this.traceList[0].traceId;
    }
  }

  attachTrace() {
    this.isSpinnerVisible = true;
    this.isButtonDisabled = true;
    const trace: TraceDto = this.traceList.find(
      (t) => t.traceId === this.selectedTrace
    );
    const cmd = new AttachTraceDto();
    cmd.TraceId = trace.traceId;
    cmd.OtauPortDto = this.dataStorage.selectedPort;
    console.log(cmd);
    this.portApiService.attachTrace(cmd).subscribe((res: string) => {
      this.isSpinnerVisible = false;
      this.isButtonDisabled = false;
      console.log(res);
      if (res === null) {
        this.router.navigate(["/rtu-tree"]);
      } else {
        this.resultMessage = res;
      }
    });
  }

  cancel() {
    this.router.navigate(["/rtu-tree"]);
  }
}
