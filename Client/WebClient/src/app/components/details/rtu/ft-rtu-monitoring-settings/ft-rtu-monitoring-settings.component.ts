import { Component, OnInit, ViewChild } from "@angular/core";
import { RtuMonitoringSettingsDto } from "src/app/models/dtos/rtu/rtuMonitoringSettingsDto";
import { ActivatedRoute } from "@angular/router";
import { Frequency } from "src/app/models/enums/frequency";
import { FrequencyPipe } from "src/app/pipes/frequency.pipe";
import { MonitoringMode } from "src/app/models/enums/monitoringMode";
import { FtRtuMonitoringPortsComponent } from "../ft-rtu-monitoring-ports/ft-rtu-monitoring-ports.component";
import { PortMonitoringMode } from "src/app/models/enums/portMonitoringMode";
import { RequestAnswer } from "src/app/models/underlying/requestAnswer";
import { ReturnCode } from "src/app/models/enums/returnCode";
import { OneApiService } from "src/app/api/one.service";
import {
  FtMessageBox,
  MessageBoxButton,
  MessageBoxStyle,
} from "src/app/components/ft-simple-dialog/ft-message-box";
import { MatDialog } from "@angular/material";
import { TranslateService } from "@ngx-translate/core";

@Component({
  selector: "ft-rtu-monitoring-settings",
  templateUrl: "./ft-rtu-monitoring-settings.component.html",
  styleUrls: ["./ft-rtu-monitoring-settings.component.css"],
})
export class FtRtuMonitoringSettingsComponent implements OnInit {
  @ViewChild(FtRtuMonitoringPortsComponent, { static: false })
  private portTableComponent: FtRtuMonitoringPortsComponent;
  public isSpinnerVisible = true;
  public isButtonDisabled = true;
  public initializationMessage: string;
  vm: RtuMonitoringSettingsDto = new RtuMonitoringSettingsDto();

  itemsSourceSave;
  itemsSourceMeas;
  oneItemSource;
  selectedPreciseMeas;
  selectedPreciseSave;
  selectedFastMeas;
  selectedFastSave;

  monitoringMode;

  constructor(
    private activeRoute: ActivatedRoute,
    private oneApiService: OneApiService,
    private frequencyPipe: FrequencyPipe,
    private ts: TranslateService,
    private matDialog: MatDialog
  ) {
    this.isSpinnerVisible = true;
    this.isButtonDisabled = false;
  }

  ngOnInit() {
    const frs = Object.keys(Frequency)
      .filter((e) => !isNaN(+e))
      .map((e) => {
        return { index: +e, name: this.frequencyPipe.transform(+e, true) };
      });
    this.itemsSourceSave = frs;

    const frm = Object.keys(Frequency)
      .filter((e) => !isNaN(+e) && (+e <= 24 || +e >= 9999))
      .map((e) => {
        return { index: +e, name: this.frequencyPipe.transform(+e, false) };
      });
    this.itemsSourceMeas = frm;

    const id = this.activeRoute.snapshot.paramMap.get("id");
    this.oneApiService
      .getRequest(`rtu/monitoring-settings/${id}`)
      .subscribe((res: RtuMonitoringSettingsDto) => {
        console.log("rtu monitoring settings received");
        this.vm = res;

        this.selectedPreciseMeas = res.preciseMeas;
        this.selectedPreciseSave = res.preciseSave;
        this.selectedFastMeas = "pp";
        this.selectedFastSave = res.fastSave;

        this.monitoringMode = res.monitoringMode === MonitoringMode.On ? 0 : 1;
        this.isSpinnerVisible = false;
      });
  }

  onButtonClicked() {
    if (this.monitoringMode === 0 && this.portTableComponent.cycleTime === 0) {
      FtMessageBox.showAndGoAlong(
        this.matDialog,
        this.ts.instant("SID_No_traces_selected_for_monitoring_"),
        this.ts.instant("SID_Error_"),
        "",
        MessageBoxButton.Ok,
        false,
        MessageBoxStyle.Full,
        "600px"
        // ).subscribe((res) => {
        //   console.log(res);
        // });
      );
      return;
    }

    this.whileRequestView();
    const dto = new RtuMonitoringSettingsDto();
    dto.rtuMaker = this.vm.rtuMaker;
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
    this.oneApiService
      .postRequest(`rtu/monitoring-settings/${id}`, dto)
      .subscribe((res: RequestAnswer) => {
        console.log(res);
        if (
          res.returnCode === ReturnCode.MonitoringSettingsAppliedSuccessfully
        ) {
          console.log("Successfully!");
        }
        this.standardView();
      });
  }

  whileRequestView() {
    this.initializationMessage = "";
    this.isSpinnerVisible = true;
    this.isButtonDisabled = true;
  }

  standardView() {
    this.isSpinnerVisible = false;
    this.isButtonDisabled = false;
  }
}
