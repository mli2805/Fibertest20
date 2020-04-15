import { Component, OnInit } from "@angular/core";
import { FtDetachedTracesProvider } from "src/app/providers/ft-detached-traces-provider";
import { RtuApiService } from "src/app/api/rtu.service";
import { TreeOfAcceptableVeasParams } from "src/app/models/dtos/meas-params/acceptableMeasParams";

@Component({
  selector: "ft-port-measurement-client",
  templateUrl: "./ft-port-measurement-client.component.html",
  styleUrls: ["./ft-port-measurement-client.component.css"],
})
export class FtPortMeasurementClientComponent implements OnInit {
  constructor(
    private dataStorage: FtDetachedTracesProvider,
    private rtuApiService: RtuApiService
  ) {}

  ngOnInit() {
    const trace = this.dataStorage.data["trace"];
    console.log(trace);
    this.rtuApiService
      .getOneRtu(trace.rtuId, "measurement-parameters")
      .subscribe((res: TreeOfAcceptableVeasParams) => {
        console.log(res);
      });
  }
}
