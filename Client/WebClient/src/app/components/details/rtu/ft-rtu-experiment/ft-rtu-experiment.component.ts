import { Component, OnInit } from "@angular/core";
import { SelectionModel } from "@angular/cdk/collections";
import { MatTableDataSource } from "@angular/material/table";
import { PortMonitoringMode } from "src/app/models/enums/portMonitoringMode";
import { RtuApiService } from "src/app/api/rtu.service";
import { ActivatedRoute } from "@angular/router";
import { RtuMonitoringSettingsDto } from "src/app/models/dtos/rtu/rtuMonitoringSettingsDto";

export interface PortLine {
  portMonitoringMode: PortMonitoringMode;
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
  tableData: PortLine[];

  displayedColumns: string[] = ["select", "port", "traceTitle", "duration"];
  dataSource = new MatTableDataSource<PortLine>();
  selectionModel = new SelectionModel<PortLine>(true, []);

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
        this.dataSource = new MatTableDataSource<PortLine>(this.tableData);
        this.selectionModel = new SelectionModel<PortLine>(true, []);
      });
  }

  /** Whether the number of selected elements matches the total number of enabled rows. */
  isAllSelected() {
    const numSelected = this.selectionModel.selected.length;
    const numRows = this.tableData.filter(l => !l.disabled).length;
    return numSelected === numRows;
  }

  /** Selects all rows if they are not all selected; otherwise clear selection. */
  masterToggle() {
    this.isAllSelected()
      ? this.selectionModel.clear()
      : this.dataSource.data.forEach(row => {
          if (!row.disabled) {
            this.selectionModel.select(row);
          }
        });
  }

  slaveToggle(row: PortLine) {
    if (row.disabled) {
      return;
    }
    this.selectionModel.toggle(row);
  }

  private createPortLines(res: RtuMonitoringSettingsDto) {
    this.tableData = res.lines.map(l => {
      return {
        portMonitoringMode: l.portMonitoringMode,
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
