import { Component, OnInit } from "@angular/core";
import { RtuMonitoringSettingsDto } from "src/app/models/dtos/rtu/rtuMonitoringSettingsDto";
import { ActivatedRoute } from "@angular/router";
import { RtuApiService } from "src/app/api/rtu.service";
import { Frequency } from "src/app/models/enums/frequency";
import { FrequencyPipe } from "src/app/pipes/frequency.pipe";
import { SelectionModel } from "@angular/cdk/collections";
import { PortMonitoringMode } from "src/app/models/enums/portMonitoringMode";
import { MatTableDataSource } from "@angular/material/table";
import { MonitoringMode } from "src/app/models/enums/monitoringMode";
import { PortLineVm } from "./portLineVm";

@Component({
  selector: "ft-rtu-monitoring-settings",
  templateUrl: "./ft-rtu-monitoring-settings.component.html",
  styleUrls: ["./ft-rtu-monitoring-settings.component.css"]
})
export class FtRtuMonitoringSettingsComponent implements OnInit {
  vm: RtuMonitoringSettingsDto = new RtuMonitoringSettingsDto();
  itemsSource;
  oneItemSource;
  selectedPreciseMeas;
  selectedPreciseSave;
  selectedFastMeas;
  selectedFastSave;

  tableData: PortLineVm[];

  displayedColumns: string[] = ["select", "port", "traceTitle", "duration"];
  dataSource = new MatTableDataSource<PortLineVm>();
  selectionModel = new SelectionModel<PortLineVm>(true, []);

  cycleTime;
  monitoringMode;

  constructor(
    private activeRoute: ActivatedRoute,
    private rtuApiService: RtuApiService,
    private frequencyPipe: FrequencyPipe
  ) {}

  ngOnInit() {
    const frs = Object.keys(Frequency)
      .filter(e => !isNaN(+e))
      .map(e => {
        return { index: +e, name: this.frequencyPipe.transform(+e) };
      });
    this.itemsSource = frs;

    const id = this.activeRoute.snapshot.paramMap.get("id");
    this.rtuApiService
      .getOneRtu(id, "monitoring-settings")
      .subscribe((res: RtuMonitoringSettingsDto) => {
        console.log("rtu monitoring settings received");
        this.vm = res;
        this.selectedPreciseMeas = res.preciseMeas;
        this.selectedPreciseSave = res.preciseSave;
        this.selectedFastMeas = "pp";
        this.selectedFastSave = res.fastSave;
        this.createPortLines();
        this.dataSource = new MatTableDataSource<PortLineVm>(this.tableData);
        this.selectionModel = new SelectionModel<PortLineVm>(
          true,
          this.tableData.filter(
            l => l.portMonitoringMode === PortMonitoringMode.On
          )
        );
        this.cycleTime = this.evaluateCycleTime();
        this.monitoringMode = res.monitoringMode === MonitoringMode.On ? 0 : 1;
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

  private createPortLines() {
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
}
