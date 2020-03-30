import { Component, OnInit, ViewChild } from "@angular/core";
import { RtuApiService } from "src/app/api/rtu.service";
import { RtuDto } from "src/app/models/dtos/rtuTree/rtuDto";
import { ChildType } from "src/app/models/enums/childType";
import { TraceDto } from "src/app/models/dtos/rtuTree/traceDto";
import { OtauWebDto } from "src/app/models/dtos/rtuTree/otauWebDto";
import { Router } from "@angular/router";
import { MatMenuTrigger } from '@angular/material';

@Component({
  selector: "ft-rtu-tree",
  templateUrl: "./ft-rtu-tree.component.html",
  styleUrls: ["./ft-rtu-tree.component.css"]
})
export class FtRtuTreeComponent implements OnInit {
  private rtus: RtuDto[];
  public isNotLoaded = true;

  @ViewChild(MatMenuTrigger, null)
  contextMenu: MatMenuTrigger;
  contextMenuPosition = { x: "0px", y: "0px" };

  constructor(private rtuService: RtuApiService, private router: Router) {
    this.isNotLoaded =  true;
  }

  ngOnInit() {
    this.isNotLoaded =  true;
    this.rtuService.getAllRtu().subscribe((res: RtuDto[]) => {
      console.log("rtu tree received", res);
      this.rtus = res;
      this.applyRtuMonitoringModeToTraces();
      this.isNotLoaded = false;
    });
  }

  back() {
    console.log("window.history.length", window.history.length);
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

  onContextMenu(event: MouseEvent) {
    event.preventDefault();
    this.contextMenuPosition.x = event.clientX + "px";
    this.contextMenuPosition.y = event.clientY + "px";
    this.contextMenu.menuData = { item: "TODO: how to know on which RTU was clicked" };
    this.contextMenu.openMenu();
    this.contextMenu.focus("mouse");
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

  scrollExperiment(rtu: RtuDto) {
    this.router.navigate(["/rtu-scroll-experiment", rtu.rtuId]);
  }

  experiment2(rtu: RtuDto) {
    this.router.navigate(["/rtu-experiment2", rtu.rtuId]);
  }
}
