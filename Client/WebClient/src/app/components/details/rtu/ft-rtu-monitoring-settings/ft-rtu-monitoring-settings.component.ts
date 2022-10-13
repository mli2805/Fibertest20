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
import { RegistrationAnswerDto } from "src/app/models/dtos/registrationAnswerDto";
import { Role } from "src/app/models/enums/role";

@Component({
  selector: "ft-rtu-monitoring-settings",
  templateUrl: "./ft-rtu-monitoring-settings.component.html",
  styleUrls: ["./ft-rtu-monitoring-settings.component.css"],
})
export class FtRtuMonitoringSettingsComponent implements OnInit {
  @ViewChild(FtRtuMonitoringPortsComponent, { static: false })
  private portTableComponent: FtRtuMonitoringPortsComponent;
  public isSpinnerVisible = false;
  public isFormDisabled = true;
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
  user: RegistrationAnswerDto;

  constructor(
    private activeRoute: ActivatedRoute,
    private oneApiService: OneApiService,
    private frequencyPipe: FrequencyPipe,
    private ts: TranslateService,
    private matDialog: MatDialog
  ) {
    this.isFormDisabled = true;
  }

  async ngOnInit() {
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
    const res = (await this.oneApiService
      .getRequest(`rtu/monitoring-settings/${id}`)
      .toPromise()) as RtuMonitoringSettingsDto;
    console.log("rtu monitoring settings received");
    this.vm = res;
    console.log(this.vm);

    this.selectedPreciseMeas = res.preciseMeas;
    this.selectedPreciseSave = res.preciseSave;
    this.selectedFastMeas = "pp";
    this.selectedFastSave = res.fastSave;

    this.monitoringMode = res.monitoringMode === MonitoringMode.On ? 0 : 1;

    this.user = JSON.parse(sessionStorage.getItem("currentUser"));
    this.isFormDisabled = this.user.role > Role.WebOperator;
  }

  async onButtonClicked() {
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
      );
      return;
    }

    this.whileRequestView();
    const dto = new RtuMonitoringSettingsDto();
    dto.connectionId = this.user.connectionId;
    dto.rtuMaker = this.vm.rtuMaker;
    dto.otdrId = this.vm.otdrId;
    dto.otauId = this.vm.otauId;
    dto.monitoringMode =
      this.monitoringMode === 0 ? MonitoringMode.On : MonitoringMode.Off;

    dto.fastSave = this.selectedFastSave;
    dto.preciseMeas = this.selectedPreciseMeas;
    dto.preciseSave = this.selectedPreciseSave;

    const isPortOns = this.portTableComponent.getPortLines();
    console.log(isPortOns);

    dto.lines = [];
    for (let i = 0; i < isPortOns.length; i++) {
      this.vm.lines[i].portMonitoringMode = isPortOns[i] ? PortMonitoringMode.On : PortMonitoringMode.Off;
      if (isPortOns[i]) {
        dto.lines.push(this.vm.lines[i]);
      }
    }

    console.log(`Apply monitoring settings dto:`);
    console.log(dto);

    const id = this.activeRoute.snapshot.paramMap.get("id");
    const res = (await this.oneApiService
      .postRequest(`rtu/monitoring-settings/${id}`, dto)
      .toPromise()) as RequestAnswer;
    console.log(res);
    if (res.returnCode === ReturnCode.MonitoringSettingsAppliedSuccessfully) {
      console.log("Successfully!");
    }
    this.standardView();
  }

  whileRequestView() {
    this.initializationMessage = "";
    this.isSpinnerVisible = true;
    this.isFormDisabled = true;
  }

  standardView() {
    this.isSpinnerVisible = false;
    this.isFormDisabled = false;
  }
}
