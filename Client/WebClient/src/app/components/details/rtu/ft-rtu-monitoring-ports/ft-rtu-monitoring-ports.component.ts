import { Component, OnInit, Input } from "@angular/core";
import { RtuMonitoringSettingsDto } from "src/app/models/dtos/rtu/rtuMonitoringSettingsDto";
import { PortLine } from "../ft-rtu-monitoring-settings/ft-rtu-monitoring-settings.component";
import { MatTableDataSource } from "@angular/material";
import { SelectionModel } from "@angular/cdk/collections";
import { PortMonitoringMode } from "src/app/models/enums/portMonitoringMode";

@Component({
  selector: "ft-rtu-monitoring-ports",
  templateUrl: "./ft-rtu-monitoring-ports.component.html",
  styleUrls: ["./ft-rtu-monitoring-ports.component.css"]
})
export class FtRtuMonitoringPortsComponent implements OnInit {
  @Input() vm: RtuMonitoringSettingsDto;

  tableData: PortLine[];

  displayedColumns: string[] = ["select", "port", "traceTitle", "duration"];
  dataSource = new MatTableDataSource<PortLine>();
  selectionModel = new SelectionModel<PortLine>(true, []);

  cycleTime;

  constructor() {}

  ngOnInit() {
    console.log(this.vm);
    if (this.vm === null || this.vm.lines == null) {
      return;
    }
    this.createPortLines();
    this.dataSource = new MatTableDataSource<PortLine>(this.tableData);
    this.selectionModel = new SelectionModel<PortLine>(
      true,
      this.tableData.filter(l => l.portMonitoringMode === PortMonitoringMode.On)
    );
    this.cycleTime = this.evaluateCycleTime();
  }

  private createPortLines() {
    console.log("createPortLines");
    this.tableData = this.vm.lines.map(l => {
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
    this.cycleTime = this.evaluateCycleTime();
  }

  slaveToggle(row: PortLine) {
    if (row.disabled) {
      return;
    }
    this.selectionModel.toggle(row);
    this.cycleTime = this.evaluateCycleTime();
  }

  private evaluateCycleTime(): number {
    if (this.selectionModel.selected.length < 1) {
      return 0;
    }
    const baseRefSum = this.selectionModel.selected.reduce(
      (a, p) => a + p.durationOfFastBase,
      0
    );
    return baseRefSum + this.selectionModel.selected.length * 2;
  }
}
