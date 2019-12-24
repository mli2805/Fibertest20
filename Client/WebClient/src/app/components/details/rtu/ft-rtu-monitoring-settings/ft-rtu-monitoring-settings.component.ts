import { Component, OnInit } from "@angular/core";
import { RtuMonitoringSettingsDto } from "src/app/models/dtos/rtu/rtuMonitoringSettingsDto";
import { ActivatedRoute } from "@angular/router";
import { RtuApiService } from "src/app/api/rtu.service";
import { Frequency } from "src/app/models/enums/frequency";
import { FrequencyPipe } from "src/app/pipes/frequency.pipe";
import { MonitoringMode } from "src/app/models/enums/monitoringMode";

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
}
