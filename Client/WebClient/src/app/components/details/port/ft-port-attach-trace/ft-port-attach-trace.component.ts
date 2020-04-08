import { Component, OnInit, Input } from "@angular/core";
import { TraceDto } from "src/app/models/dtos/rtuTree/traceDto";
import { FtDetachedTracesProvider } from "src/app/providers/ft-detached-traces";
import { RtuDto } from "src/app/models/dtos/rtuTree/rtuDto";
import { ChildDto } from "src/app/models/dtos/rtuTree/childDto";

@Component({
  selector: "ft-port-attach-trace",
  templateUrl: "./ft-port-attach-trace.component.html",
  styleUrls: ["./ft-port-attach-trace.component.css"],
})
export class FtPortAttachTraceComponent implements OnInit {
  traceList: TraceDto[];

  constructor(private dataStorage: FtDetachedTracesProvider) {}

  ngOnInit() {
    const rtu: RtuDto = this.dataStorage.data;
    this.traceList = rtu.children
      .filter((c) => c.childType === 1 && c.port === -1)
      .map((ch: ChildDto) => ch as TraceDto);

    console.log(this.traceList);
  }
}
