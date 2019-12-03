import { Component, OnInit } from "@angular/core";
import { RtuApiService } from "src/app/api/rtu.service";
import { RtuDto } from "src/app/models/dtos/rtuDto";
import { ChildType } from "src/app/models/enums/childType";
import { TraceDto } from "src/app/models/dtos/traceDto";
import { OtauWebDto } from "src/app/models/dtos/otauWebDto";
import { throwToolbarMixedModesError } from "@angular/material";

@Component({
  selector: "ft-rtu-tree",
  templateUrl: "./ft-rtu-tree.component.html",
  styleUrls: ["./ft-rtu-tree.component.css"]
})
export class FtRtuTreeComponent implements OnInit {
  private rtus: RtuDto[];

  constructor(private rtuService: RtuApiService) {}

  ngOnInit() {
    this.rtuService.getAllRtu().subscribe((res: RtuDto[]) => {
      console.log("rtu tree received");
      this.rtus = res;
      this.applyRtuMonitoringModeToTraces();
    });
  }

  applyRtuMonitoringModeToTraces() {
    for (const rtu of this.rtus) {
      for (const child of rtu.children) {
        if (child.childType === ChildType.Trace) {
          const trace = child as TraceDto;
          trace.rtuMonitoringMode = rtu.monitoringMode;
        }

        if (child.childType === ChildType.Otau) {
          const otau = child as OtauWebDto;
          for (const otauChild of otau.children) {
            if (otauChild.childType === ChildType.Trace) {
              const trace = otauChild as TraceDto;
              trace.rtuMonitoringMode = rtu.monitoringMode;
            }
          }
        }
      }
    }
  }
}
