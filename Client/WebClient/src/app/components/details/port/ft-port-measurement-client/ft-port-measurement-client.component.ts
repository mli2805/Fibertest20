import { Component, OnDestroy, OnInit } from "@angular/core";
import { TreeOfAcceptableVeasParams } from "src/app/models/dtos/meas-params/acceptableMeasParams";
import { Router } from "@angular/router";
import {
  DoClientMeasurementDto,
  MeasParam,
} from "src/app/models/dtos/meas-params/doClientMeasurementDto";
import { RequestAnswer } from "src/app/models/underlying/requestAnswer";
import { ReturnCode } from "src/app/models/enums/returnCode";
import { SignalrService } from "src/app/api/signalr.service";
import { ClientMeasurementDoneDto } from "src/app/models/dtos/port/clientMeasurementDoneDto";
import { TranslateService } from "@ngx-translate/core";
import { OneApiService } from "src/app/api/one.service";
import { SorFileManager } from "src/app/utils/sorFileManager";

@Component({
  selector: "ft-port-measurement-client",
  templateUrl: "./ft-port-measurement-client.component.html",
  styleUrls: ["./ft-port-measurement-client.component.css"],
})
export class FtPortMeasurementClientComponent implements OnInit, OnDestroy {
  tree: TreeOfAcceptableVeasParams;

  message;
  isSpinnerVisible = false;
  isButtonDisabled = false;

  itemsSourceUnits;
  itemsSourceDistances;
  itemsSourceResolutions;
  itemsSourcePulseDurations;
  itemsSourcePeriodsToAverages;
  selectedUnit;
  selectedDistance;
  selectedResolution;
  selectedPulseDuration;
  selectedPeriodToAverage;
  selectedBc = 0;
  selectedRi = 0;

  measEmmitterSubscription;

  constructor(
    private router: Router,
    private oneApiService: OneApiService,
    private signalRService: SignalrService,
    private ts: TranslateService
  ) {}

  ngOnDestroy(): void {
    this.measEmmitterSubscription.unsubscribe();
  }

  async ngOnInit() {
    console.log("we are in ngOnInit of measurement client component");
    this.isSpinnerVisible = true;
    const rtuId = JSON.parse(sessionStorage.getItem("measurementClientParams"))
      .rtuId;
    this.tree = (await this.oneApiService
      .getRequest(`rtu/measurement-parameters/${rtuId}`)
      .toPromise()) as TreeOfAcceptableVeasParams;

    console.log("tree: ", this.tree);
    this.initializeLists();
    this.isSpinnerVisible = false;
    this.isButtonDisabled = false;
    this.message = "";

    this.measEmmitterSubscription = this.signalRService.clientMeasEmitter.subscribe(
      (signal: ClientMeasurementDoneDto) => {
        console.log(signal);
        const currentUser = JSON.parse(sessionStorage.currentUser);
        if (signal.connectionId !== currentUser.connectionId) {
          console.log(`It is measurement for another web client`);
          return;
        }

        if (signal.returnCode === ReturnCode.MeasurementEndedNormally) {
          this.message = this.ts.instant("SID_Measurement_is_finished_");
        } else {
          this.message = "Measurement (Client) failed!";
        }
        this.isSpinnerVisible = false;
        this.isButtonDisabled = false;
      }
    );
  }

  initializeLists() {
    const units = Object.keys(this.tree["units"]);
    this.itemsSourceUnits = units;
    this.selectedUnit = units[0];

    const selectedUnitBranch = this.tree["units"][this.selectedUnit];
    this.selectedBc = selectedUnitBranch["backscatteredCoefficient"];
    this.selectedRi = selectedUnitBranch["refractiveIndex"];
    const distances = selectedUnitBranch["distances"];

    const distancesKeys = Object.keys(distances);
    this.itemsSourceDistances = distancesKeys;
    this.selectedDistance = distancesKeys[0];

    const leaf = distances[this.selectedDistance];

    this.itemsSourceResolutions = leaf["resolutions"];
    this.selectedResolution = leaf["resolutions"][0];

    this.itemsSourcePulseDurations = leaf["pulseDurations"];
    this.selectedPulseDuration = leaf["pulseDurations"][0];

    this.itemsSourcePeriodsToAverages = leaf["periodsToAverage"];
    this.selectedPeriodToAverage = leaf["periodsToAverage"][0];
  }

  unitChanged() {
    const selectedUnitBranch = this.tree["units"][this.selectedUnit];
    const distances = selectedUnitBranch["distances"];

    this.itemsSourceDistances = Object.keys(distances);
    if (!distances.includes(this.selectedDistance)) {
      this.selectedDistance = selectedUnitBranch["distances"][0];
      this.distanceChanged();
    }
  }

  distanceChanged() {
    const distances = this.tree["units"][this.selectedUnit]["distances"];
    const leaf = distances[this.selectedDistance];

    this.itemsSourceResolutions = leaf["resolutions"];
    if (!leaf["resolutions"].includes(this.selectedResolution)) {
      this.selectedResolution = leaf["resolutions"][0];
    }

    this.itemsSourcePulseDurations = leaf["pulseDurations"];
    if (!leaf["pulseDurations"].includes(this.selectedPulseDuration)) {
      this.selectedPulseDuration = leaf["pulseDurations"][0];
    }

    this.itemsSourcePeriodsToAverages = leaf["periodsToAverage"];
    if (!leaf["periodsToAverage"].includes(this.selectedPeriodToAverage)) {
      this.selectedPeriodToAverage = leaf["periodsToAverage"][0];
    }
  }

  getSelectedParameters() {
    const params: MeasParam[] = new Array(8);
    params[0] = new MeasParam(
      1,
      this.itemsSourceUnits.indexOf(this.selectedUnit)
    );
    params[1] = new MeasParam(11, Math.round(this.selectedBc * 100));
    params[2] = new MeasParam(10, this.selectedRi * 100000);
    params[3] = new MeasParam(
      2,
      this.itemsSourceDistances.indexOf(this.selectedDistance)
    );
    params[4] = new MeasParam(
      5,
      this.itemsSourceResolutions.indexOf(this.selectedResolution)
    );
    params[5] = new MeasParam(
      6,
      this.itemsSourcePulseDurations.indexOf(this.selectedPulseDuration)
    );
    params[6] = new MeasParam(9, 1);
    params[7] = new MeasParam(
      8,
      this.itemsSourcePeriodsToAverages.indexOf(this.selectedPeriodToAverage)
    );
    return params;
  }

  async measure() {
    this.isSpinnerVisible = true;
    this.isButtonDisabled = true;
    this.message = this.ts.instant("SID_Sending_command__Wait_please___");
    const params = JSON.parse(
      sessionStorage.getItem("measurementClientParams")
    );
    const dto = new DoClientMeasurementDto();
    const currentUser = JSON.parse(sessionStorage.currentUser);
    dto.connectionId = currentUser.connectionId;
    dto.rtuId = params.rtuId;
    dto.otauPortDto = params.otauPortDto;
    dto.selectedMeasParams = this.getSelectedParameters();
    console.log(dto);
    const res = (await this.oneApiService
      .postRequest("measurement/measurement-client", dto)
      .toPromise()) as RequestAnswer;
    if (res.returnCode !== ReturnCode.Ok) {
      this.message = res.errorMessage;
      this.isSpinnerVisible = false;
      this.isButtonDisabled = false;
    } else {
      this.message = this.ts.instant(
        "SID_Measurement__Client__in_progress__Please_wait___"
      );
    }
  }

  close() {
    this.router.navigate(["/ft-main-nav/rtu-tree"]);
  }
}
