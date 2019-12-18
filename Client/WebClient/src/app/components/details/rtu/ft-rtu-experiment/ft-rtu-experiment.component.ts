import { Component, OnInit } from "@angular/core";
import { SelectionModel } from "@angular/cdk/collections";
import { MatTableDataSource } from "@angular/material/table";
import { PortMonitoringMode } from "src/app/models/enums/portMonitoringMode";
import { RtuApiService } from "src/app/api/rtu.service";
import { ActivatedRoute } from "@angular/router";
import { RtuMonitoringSettingsDto } from "src/app/models/dtos/rtu/rtuMonitoringSettingsDto";

export interface PortLine {
  portMonitoringMode: PortMonitoringMode;
  position: number;
  port: string;
  disabled: boolean;
  traceTitle: string;
  durationOfPreciseBase: number;
  durationOfFastBase: number;
  duration: string;
}

@Component({
  selector: "ft-rtu-experiment",
  templateUrl: "./ft-rtu-experiment.component.html",
  styleUrls: ["./ft-rtu-experiment.component.css"]
})
export class FtRtuExperimentComponent implements OnInit {
  ELEMENT_DATA2: PortLine[];

  displayedColumns2: string[] = ["select", "port", "traceTitle", "duration"];
  dataSource2 = new MatTableDataSource<PortLine>();
  selection2 = new SelectionModel<PortLine>(true, []);

  constructor(
    private activeRoute: ActivatedRoute,
    private rtuApiService: RtuApiService
  ) {}

  ngOnInit() {
    const id = this.activeRoute.snapshot.paramMap.get("id");
    this.rtuApiService
      .getOneRtu(id, "monitoring-settings")
      .subscribe((res: RtuMonitoringSettingsDto) => {
        console.log("rtu monitoring settings received");
        this.createPortLines(res);
        this.dataSource2 = new MatTableDataSource<PortLine>(this.ELEMENT_DATA2);
        this.selection2 = new SelectionModel<PortLine>(true, []);
      });
  }

  /** Whether the number of selected elements matches the total number of rows. */
  isAllSelected() {
    const numSelected = this.selection2.selected.length;
    const numRows = this.ELEMENT_DATA2.filter(l => !l.disabled).length;
    return numSelected === numRows;
  }

  /** Selects all rows if they are not all selected; otherwise clear selection. */
  masterToggle() {
    this.isAllSelected()
      ? this.selection2.clear()
      : this.dataSource2.data.forEach(row => {
          if (!row.disabled) {
            this.selection2.select(row);
          }
        });
  }

  slaveToggle(row: PortLine) {
    if (row.disabled) {
      return;
    }
    this.selection2.toggle(row);
  }

  private createPortLines(res: RtuMonitoringSettingsDto) {
    this.ELEMENT_DATA2 = res.lines.map(l => {
      return {
        portMonitoringMode: l.portMonitoringMode,
        position: 1,
        disabled:
          l.portMonitoringMode === PortMonitoringMode.NoTraceJoined ||
          l.portMonitoringMode === PortMonitoringMode.TraceHasNoBase,
        port: l.port,
        traceTitle: l.traceTitle,
        durationOfFastBase: l.durationOfFastBase,
        durationOfPreciseBase: l.durationOfPreciseBase,
        duration: `${l.durationOfFastBase} / ${l.durationOfPreciseBase} sec`
      };
    });
  }
}
