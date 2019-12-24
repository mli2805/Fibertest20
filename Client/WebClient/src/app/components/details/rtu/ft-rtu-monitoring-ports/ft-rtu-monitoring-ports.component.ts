import { Component, OnInit, Input, OnChanges } from "@angular/core";
import { RtuMonitoringPortDto } from "src/app/models/dtos/rtu/rtuMonitoringSettingsDto";
import { MatTableDataSource } from "@angular/material";
import { SelectionModel } from "@angular/cdk/collections";
import { PortMonitoringMode } from "src/app/models/enums/portMonitoringMode";
import { PortLineVm } from "../ft-rtu-monitoring-settings/portLineVm";

@Component({
  selector: "ft-rtu-monitoring-ports",
  templateUrl: "./ft-rtu-monitoring-ports.component.html",
  styleUrls: ["./ft-rtu-monitoring-ports.component.css"]
})
export class FtRtuMonitoringPortsComponent implements OnInit, OnChanges {
  @Input() ports: RtuMonitoringPortDto[];

  tableData: PortLineVm[];

  displayedColumns: string[] = ["select", "port", "traceTitle", "duration"];
  dataSource = new MatTableDataSource<PortLineVm>();
  selectionModel = new SelectionModel<PortLineVm>(true, []);

  cycleTime;

  constructor() {}

  ngOnChanges() {
    if (this.ports == null) {
      return;
    }
    this.initializeComponent();
  }

  ngOnInit() {}

  private initializeComponent() {
    this.createPortLines();
    console.log(this.tableData);
    this.dataSource = new MatTableDataSource<PortLineVm>(this.tableData);
    this.selectionModel = new SelectionModel<PortLineVm>(
      true,
      this.tableData.filter(l => l.portMonitoringMode === PortMonitoringMode.On)
    );
    this.cycleTime = this.evaluateCycleTime();
    console.log(`${this.cycleTime} sec`);
  }

  private createPortLines() {
    this.tableData = this.ports.map(l => {
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

  slaveToggle(row: PortLineVm) {
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
