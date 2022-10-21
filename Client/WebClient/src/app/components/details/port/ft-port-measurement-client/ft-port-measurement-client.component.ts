import { Component, OnDestroy, OnInit } from "@angular/core";
import { BranchOfAcceptableMeasParams, LeafOfAcceptableMeasParams, TreeOfAcceptableVeasParams } from "src/app/models/dtos/meas-params/acceptableMeasParams";
import { Router } from "@angular/router";
import {
  AnalysisParameters,
  DoClientMeasurementDto,
  Laser,
  LasersParameter,
  LasersProperty,
  MeasParam,
  OpticalLineProperties,
  VeexMeasOtdrParameters,
} from "src/app/models/dtos/meas-params/doClientMeasurementDto";
import { RequestAnswer } from "src/app/models/underlying/requestAnswer";
import { ReturnCode } from "src/app/models/enums/returnCode";
import { SignalrService } from "src/app/api/signalr.service";
import { ClientMeasurementDoneDto } from "src/app/models/dtos/port/clientMeasurementDoneDto";
import { TranslateService } from "@ngx-translate/core";
import { OneApiService } from "src/app/api/one.service";
import { SorFileManager } from "src/app/utils/sorFileManager";
import { OtauPortDto } from "src/app/models/underlying/otauPortDto";
import { GetClientMeasurementDto } from "src/app/models/dtos/meas-params/getClientMeasurementDto";
import { ClientMeasurementVeexResultDto } from "src/app/models/dtos/meas-params/clientMeasurementVeexResultDto";
import { RtuMaker } from "src/app/models/enums/rtuMaker";
import { Console } from "console";
import { ClientMeasurementStartedDto } from "src/app/models/dtos/meas-params/clientMeasurementStartedDto";
import { ReturnCodePipe } from "src/app/pipes/return-code.pipe";
import { OccupyRtuDto, RtuOccupationState, RtuOccupation } from "src/app/models/dtos/meas-params/occupyRtuDto";

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

  params;
  currentUser;

  constructor(
    private router: Router,
    private oneApiService: OneApiService,
    private signalRService: SignalrService,
    private ts: TranslateService,
    private returnCodePipe: ReturnCodePipe
  ) {}

  ngOnDestroy(): void {
    this.measEmmitterSubscription.unsubscribe();
  }

  async ngOnInit() {
    console.log("we are in ngOnInit of measurement client component");
    this.isSpinnerVisible = true;
    this.params = JSON.parse(sessionStorage.getItem("measurementClientParams"));
    this.currentUser = JSON.parse(sessionStorage.currentUser);
    const rtuId = this.params.selectedRtu.rtuId;
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
        if (signal.connectionId !== this.currentUser.connectionId) {
          console.log(`It is measurement for another web client`);
          return;
        }

        if (signal.returnCode === ReturnCode.MeasurementEndedNormally) {
          this.message = this.ts.instant("SID_Measurement_is_finished_");
        } else {
          this.message = "Measurement (Client) failed!";
        }

        this.deOccupyRtu(); // iit rtu
        this.isSpinnerVisible = false;
        this.isButtonDisabled = false;
      }
    );
  }

  async deOccupyRtu(){
    var freeDto = new OccupyRtuDto();
    freeDto.connectionId = this.currentUser.connectionId;
    freeDto.rtuId = this.params.selectedRtu.rtuId;
    freeDto.state = new RtuOccupationState();
    freeDto.state.rtuId = this.params.selectedRtu.rtuId;
    freeDto.state.rtuOccupation = RtuOccupation.None;

    const res = (await this.oneApiService
      .postRequest("rtu/set-rtu-occupation-state", freeDto)
      .toPromise()) as RequestAnswer;
    console.log(`${this.returnCodePipe.transform(res.returnCode)}`);
  }

  initializeLists() {
    const units = Object.keys(this.tree.units);
    this.itemsSourceUnits = units;
    this.selectedUnit = units[0];

    const selectedUnitBranch : BranchOfAcceptableMeasParams = this.tree.units[this.selectedUnit];
    this.selectedBc = selectedUnitBranch["backscatteredCoefficient"];
    this.selectedRi = selectedUnitBranch["refractiveIndex"];
    const distances = selectedUnitBranch.distances;

    const distancesKeys = [];
    let sortable = [];
    for (var ddd in distances){
      sortable.push([ddd, Number(ddd)]);
    }
    sortable.sort(function(a,b){return a[1]-b[1]});
    for (var i = 0; i < sortable.length; i++){
      distancesKeys.push(sortable[i][0]);
    }

    console.log(`distancesKeys = ${distancesKeys}`);
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

  getVeexSelectedParameters(){
    const params: VeexMeasOtdrParameters = new VeexMeasOtdrParameters();
    params.measurementType = "manual";
    params.fastMeasurement = false;
    params.highFrequencyResolution = false;
    params.lasers = [new Laser(this.selectedUnit)];

    const lp = new LasersProperty(
      // this.selectedUnit, Math.round(this.selectedBc * 100), this.selectedRi * 100000);
      this.selectedUnit, this.selectedBc, this.selectedRi);
    params.opticalLineProperties = new OpticalLineProperties("point_to_point", [lp]);

    params.distanceRange = this.selectedDistance;
    params.resolution = this.selectedResolution;
    params.pulseDuration = this.selectedPulseDuration;
    params.averagingTime = this.selectedPeriodToAverage;

    return params;
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
    dto.connectionId = this.currentUser.connectionId;
    dto.rtuId = params.selectedRtu.rtuId;
    dto.otdrId = params.selectedRtu.otdrId;
    dto.otauPortDto = [params.selectedPort];
    if (!params.selectedPort.isPortOnMainCharon){
      const mainOtau = new OtauPortDto();
      mainOtau.isPortOnMainCharon = true;
      mainOtau.otauId = params.selectedRtu.mainVeexOtau.id;
      mainOtau.opticalPort = params.selectedPort.mainCharonPort;
      dto.otauPortDto.push(mainOtau);
    }

    dto.selectedMeasParams = this.getSelectedParameters();
    dto.veexMeasOtdrParameters = this.getVeexSelectedParameters();
    dto.analysisParameters = new AnalysisParameters();
    let lp = new LasersParameter();
    lp.eventLossThreshold = 0.2;
    lp.eventReflectanceThreshold = -40;
    lp.endOfFiberThreshold = 6;
    dto.analysisParameters.lasersParameters = new Array(lp);

    dto.IsAutoLmax = false;
    dto.isForAutoBase = false;
    dto.KeepOtdrConnection = false;

    console.log(dto);
    const res = (await this.oneApiService
      .postRequest("measurement/start-measurement-client", dto)
      .toPromise()) as ClientMeasurementStartedDto;
    if (res.returnCode !== ReturnCode.MeasurementClientStartedSuccessfully) {
      this.message = this.returnCodePipe.transform(res.returnCode);
      this.isSpinnerVisible = false;
      this.isButtonDisabled = false;
    } else {
      console.log(`measurement id ${res.clientMeasurementId}`);
      this.message = this.ts.instant(
        "SID_Measurement__Client__in_progress__Please_wait___"
      );

      if (params.selectedRtu.rtuMaker == RtuMaker.IIT)
        return;

      const getDto = new GetClientMeasurementDto();
      getDto.connectionId = this.currentUser.connectionId;
      getDto.rtuId = params.selectedRtu.rtuId;
      getDto.veexMeasurementId = res.clientMeasurementId;
      var measRes: ClientMeasurementVeexResultDto;
      while (true) {
        await new Promise(f => setTimeout(f, 5000));

        measRes = (await this.oneApiService
          .getRequest("measurement/get-measurement-client-result", getDto)
          .toPromise()) as ClientMeasurementVeexResultDto;

        if (measRes.returnCode != ReturnCode.Ok) {
            this.message = `Failed to get veex measurement result`;
            break;
        }
  
        if (measRes.returnCode == ReturnCode.Ok && measRes.veexMeasurementStatus == "failed") {
            this.message = `Measurement (Client) failed!`;
            break;
        }
  
        if (measRes.returnCode == ReturnCode.Ok && measRes.veexMeasurementStatus == "finished") {
            SorFileManager.Show(this.router,  false,  0,  getDto.veexMeasurementId,
              false, "meas", new Date(), 
              JSON.parse(sessionStorage.getItem("measurementClientParams")).selectedRtu.rtuId)

            this.message = this.ts.instant("SID_Measurement_is_finished_");
            break;
        }
      }

      this.deOccupyRtu(); // veex rtu
      this.isSpinnerVisible = false;
      this.isButtonDisabled = false;
    }
  }

  close() {
    this.router.navigate(["/ft-main-nav/rtu-tree"]);
  }
}
