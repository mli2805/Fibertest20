import { Component, OnInit } from "@angular/core";
import {
  RtuMonitoringSettingsDto,
  RtuMonitoringPortDto
} from "src/app/models/dtos/rtu/rtuMonitoringSettingsDto";
import { ActivatedRoute } from "@angular/router";
import { RtuApiService } from "src/app/api/rtu.service";
import { Frequency } from "src/app/models/enums/frequency";
import { FrequencyPipe } from "src/app/pipes/frequency.pipe";
import { SelectionModel } from "@angular/cdk/collections";
import { PortMonitoringMode } from "src/app/models/enums/portMonitoringMode";

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

  portLines;
  displayedColumns = [
    "select",
    "monitoringMode",
    "port",
    "traceTitle",
    "duration"
  ];
  initialSelection = [];
  allowMultiSelect = true;
  selection = new SelectionModel<RtuMonitoringPortDto>(
    this.allowMultiSelect,
    this.initialSelection
  );

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
      });
  }

  private createPortLines() {
    this.portLines = this.vm.lines.map(l => {
      return {
        checked: l.portMonitoringMode === PortMonitoringMode.On,
        disabled:
          l.portMonitoringMode === PortMonitoringMode.NoTraceJoined ||
          l.portMonitoringMode === PortMonitoringMode.TraceHasNoBase,
        port: l.port,
        traceTitle: l.traceTitle,
        duration: `${l.durationOfFastBase} / ${l.durationOfPreciseBase} sec`
      };
    });
  }

  onClick(element: RtuMonitoringPortDto) {
    console.log("coo-coo");
  }

  /** Whether the number of selected elements matches the total number of rows. */
  isAllSelected() {
    const numSelected = this.selection.selected.length;
    const numRows = this.portLines.data.length;
    return numSelected === numRows;
  }

  /** Selects all rows if they are not all selected; otherwise clear selection. */
  masterToggle() {
    this.isAllSelected()
      ? this.selection.clear()
      : this.portLines.data.forEach(row => this.selection.select(row));
  }
}
