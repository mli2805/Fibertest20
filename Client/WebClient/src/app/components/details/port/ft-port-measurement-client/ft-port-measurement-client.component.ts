import { Component, OnInit } from "@angular/core";
import { FtDetachedTracesProvider } from "src/app/providers/ft-detached-traces-provider";
import { RtuApiService } from "src/app/api/rtu.service";
import { TreeOfAcceptableVeasParams } from "src/app/models/dtos/meas-params/acceptableMeasParams";

@Component({
  selector: "ft-port-measurement-client",
  templateUrl: "./ft-port-measurement-client.component.html",
  styleUrls: ["./ft-port-measurement-client.component.css"],
})
export class FtPortMeasurementClientComponent implements OnInit {
  vm: TreeOfAcceptableVeasParams;

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
    private dataStorage: FtDetachedTracesProvider,
    private rtuApiService: RtuApiService
  ) {}

  ngOnInit() {
    const trace = this.dataStorage.data["trace"];
    this.rtuApiService
      .getOneRtu(trace.rtuId, "measurement-parameters")
      .subscribe((res: TreeOfAcceptableVeasParams) => {
        console.log("tree: ", res);
        this.vm = res;
        this.initializeLists();
      });
  }

  /* tslint:disable:no-string-literal */
  initializeLists() {
    const tree = this.vm["units"];

    const units = Object.keys(tree);
    this.itemsSourceUnits = units;
    this.selectedUnit = units[0];

    const selectedUnitBranch = tree[this.selectedUnit];
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
  /* tslint:enable:no-string-literal */
}
