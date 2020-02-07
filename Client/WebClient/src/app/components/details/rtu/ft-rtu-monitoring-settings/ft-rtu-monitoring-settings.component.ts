import { Component, OnInit, ViewChild } from "@angular/core";
import {
  RtuMonitoringSettingsDto,
  RtuMonitoringPortDto
} from "src/app/models/dtos/rtu/rtuMonitoringSettingsDto";
import { ActivatedRoute } from "@angular/router";
import { RtuApiService } from "src/app/api/rtu.service";
import { Frequency } from "src/app/models/enums/frequency";
import { FrequencyPipe } from "src/app/pipes/frequency.pipe";
import { MonitoringMode } from "src/app/models/enums/monitoringMode";
import { FtRtuMonitoringPortsComponent } from "../ft-rtu-monitoring-ports/ft-rtu-monitoring-ports.component";
import { PortMonitoringMode } from "src/app/models/enums/portMonitoringMode";

@Component({
  selector: "ft-rtu-monitoring-settings",
  templateUrl: "./ft-rtu-monitoring-settings.component.html",
  styleUrls: ["./ft-rtu-monitoring-settings.component.css"]
})
export class FtRtuMonitoringSettingsComponent implements OnInit {
  @ViewChild(FtRtuMonitoringPortsComponent, { static: false })
  private portTableComponent: FtRtuMonitoringPortsComponent;

  vm: RtuMonitoringSettingsDto = new RtuMonitoringSettingsDto();

  itemsSource;
  oneItemSource;
  selectedPreciseMeas;
  selectedPreciseSave;
  selectedFastMeas;
  selectedFastSave;

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

        this.monitoringMode = res.monitoringMode === MonitoringMode.On ? 0 : 1;
      });
  }

  onButtonClicked() {
    console.log("button Apply monitoring settigs clicked ");
    const dto = new RtuMonitoringSettingsDto();
    dto.monitoringMode =
      this.monitoringMode === 0 ? MonitoringMode.On : MonitoringMode.Off;

    dto.fastSave = this.selectedFastSave;
    dto.preciseMeas = this.selectedPreciseMeas;
    dto.preciseSave = this.selectedPreciseSave;

    const isPortOns = this.portTableComponent.getPortLines();

    console.log(isPortOns);

    dto.lines = [];
    for (let i = 0; i < isPortOns.length; i++) {
      if (isPortOns[i]) {
        this.vm.lines[i].portMonitoringMode = PortMonitoringMode.On;
        dto.lines.push(this.vm.lines[i]);
      }
    }

    console.log(dto);

    const id = this.activeRoute.snapshot.paramMap.get("id");
    this.rtuApiService
      .postOneRtu(id, "monitoring-settings", dto)
      .subscribe((res: any) => {
        console.log(res);
      });
  }
}