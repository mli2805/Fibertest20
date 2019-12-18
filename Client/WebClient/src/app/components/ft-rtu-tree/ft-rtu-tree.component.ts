import { Component, OnInit } from "@angular/core";
import { RtuApiService } from "src/app/api/rtu.service";
import { RtuDto } from "src/app/models/dtos/rtuTree/rtuDto";
import { ChildType } from "src/app/models/enums/childType";
import { TraceDto } from "src/app/models/dtos/rtuTree/traceDto";
import { OtauWebDto } from "src/app/models/dtos/rtuTree/otauWebDto";
import { Router } from "@angular/router";

@Component({
  selector: "ft-rtu-tree",
  templateUrl: "./ft-rtu-tree.component.html",
  styleUrls: ["./ft-rtu-tree.component.css"]
})
export class FtRtuTreeComponent implements OnInit {
  private rtus: RtuDto[];

  constructor(private rtuService: RtuApiService, private router: Router) {}

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

  expand(rtu: RtuDto) {
    rtu.expanded = !rtu.expanded;
  }

  information(rtu: RtuDto) {
    this.router.navigate(["/rtu-information", rtu.rtuId]);
  }

  networkSettings(rtu: RtuDto) {
    this.router.navigate(["/rtu-network-settings", rtu.rtuId]);
  }

  state(rtu: RtuDto) {
    this.router.navigate(["/rtu-state", rtu.rtuId]);
  }

  monitoringSettings(rtu: RtuDto) {
    this.router.navigate(["/rtu-monitoring-settings", rtu.rtuId]);
  }

  experiment(rtu: RtuDto) {
    this.router.navigate(["/rtu-experiment", rtu.rtuId]);
  }
}
