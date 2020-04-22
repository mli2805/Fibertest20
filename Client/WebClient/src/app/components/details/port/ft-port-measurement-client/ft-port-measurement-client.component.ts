import { Component, OnInit, OnDestroy } from "@angular/core";
import { FtComponentDataProvider } from "src/app/providers/ft-component-data-provider";
import { TreeOfAcceptableVeasParams } from "src/app/models/dtos/meas-params/acceptableMeasParams";
import { Router } from "@angular/router";
import { DoClientMeasurementDto } from "src/app/models/dtos/meas-params/doClientMeasurementDto";
import { RequestAnswer } from "src/app/models/underlying/requestAnswer";
import { ReturnCode } from "src/app/models/enums/returnCode";
import { Subscription } from "rxjs";
import { SignalrService } from "src/app/api/signalr.service";
import { ClientMeasurementDoneDto } from "src/app/models/dtos/port/clientMeasurementDoneDto";
import { TranslateService } from "@ngx-translate/core";
import { OneApiService } from "src/app/api/one.service";

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

  private subscription: Subscription;

  constructor(
    private router: Router,
    private dataStorage: FtComponentDataProvider,
    private oneApiService: OneApiService,
    private signalRService: SignalrService,
    private ts: TranslateService
  ) {}

  ngOnInit() {
    this.isSpinnerVisible = true;
    const rtuId = this.dataStorage.data["rtuId"];
    this.oneApiService
      .getRequest(`rtu/measurement-parameters/${rtuId}`)
      .subscribe((res: TreeOfAcceptableVeasParams) => {
        console.log("tree: ", res);
        this.tree = res;
        this.initializeLists();
        this.isSpinnerVisible = false;
        this.isButtonDisabled = false;
        this.message = "";
      });

    this.subscription = this.signalRService.clientMeasurementEmitter.subscribe(
      (signal: ClientMeasurementDoneDto) => {
        if (signal.ReturnCode === ReturnCode.MeasurementEndedNormally) {
          this.message = `Measurement (Client) done. SorBytes contains ${signal.SorBytes.length} bytes`;
        } else {
          this.message = "Measurement (Client) failed!";
        }
        this.isSpinnerVisible = false;
        this.isButtonDisabled = false;
      }
    );
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
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
    const result = {
      1: this.selectedUnit,
      11: Math.round(this.selectedBc * 100),
      10: this.selectedRi * 100000,
      2: this.selectedDistance,
      5: this.selectedResolution,
      6: this.selectedPulseDuration,
      9: 1,
      8: this.selectedPeriodToAverage,
    };
    return result;
  }

  measure() {
    this.isSpinnerVisible = true;
    this.isButtonDisabled = true;
    this.message = this.ts.instant("SID_Sending_command__Wait_please___");
    console.log(this.getSelectedParameters());
    const dto = new DoClientMeasurementDto();
    dto.rtuId = this.dataStorage.data["rtuId"];
    dto.otauPortDto = this.dataStorage.data["otauPortDto"];
    dto.selectedMeasParams = this.getSelectedParameters();
    this.oneApiService
      .postRequest("port/measurement-client", dto)
      .subscribe((res: RequestAnswer) => {
        if (res.returnCode !== ReturnCode.Ok) {
          this.message = res.errorMessage;
          this.isSpinnerVisible = false;
          this.isButtonDisabled = false;
        } else {
          this.message = this.ts.instant(
            "SID_Measurement__Client__in_progress__Please_wait___"
          );
        }
      });
  }

  close() {
    this.router.navigate(["/rtu-tree"]);
  }
}
