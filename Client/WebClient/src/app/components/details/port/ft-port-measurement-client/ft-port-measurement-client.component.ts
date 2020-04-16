import { Component, OnInit } from "@angular/core";
import { FtDetachedTracesProvider } from "src/app/providers/ft-detached-traces-provider";
import { RtuApiService } from "src/app/api/rtu.service";
import { TreeOfAcceptableVeasParams } from "src/app/models/dtos/meas-params/acceptableMeasParams";
import { Router } from "@angular/router";
import { PortApiService } from "src/app/api/port.service";
import { DoClientMeasurementDto } from "src/app/models/dtos/meas-params/doClientMeasurementDto";

@Component({
  selector: "ft-port-measurement-client",
  templateUrl: "./ft-port-measurement-client.component.html",
  styleUrls: ["./ft-port-measurement-client.component.css"],
})
export class FtPortMeasurementClientComponent implements OnInit {
  tree: TreeOfAcceptableVeasParams;

  isSpinnerVisible = false;

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

  constructor(
    private router: Router,
    private dataStorage: FtDetachedTracesProvider,
    private rtuApiService: RtuApiService,
    private portApiService: PortApiService
  ) {}

  /* tslint:disable:no-string-literal */
  ngOnInit() {
    this.isSpinnerVisible = true;
    const rtuId = this.dataStorage.data["rtuId"];
    this.rtuApiService
      .getRequest(rtuId, "measurement-parameters")
      .subscribe((res: TreeOfAcceptableVeasParams) => {
        console.log("tree: ", res);
        this.tree = res;
        this.initializeLists();
        this.isSpinnerVisible = false;
      });
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
    console.log(this.getSelectedParameters());
    const dto = new DoClientMeasurementDto();
    dto.rtuId = this.dataStorage.data["rtuId"];
    dto.otauPortDto = this.dataStorage.data["otauPortDto"];
    dto.selectedMeasParams = this.getSelectedParameters();
    this.portApiService
      .postRequest("measurement-client", dto)
      .subscribe((res: any) => {
        console.log(res);
        this.isSpinnerVisible = false;
      });
  }

  close() {
    this.router.navigate(["/rtu-tree"]);
  }
}
