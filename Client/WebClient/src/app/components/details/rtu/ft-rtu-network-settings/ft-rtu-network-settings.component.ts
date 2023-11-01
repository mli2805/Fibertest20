import { Component, OnInit, OnDestroy } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { RtuNetworkSettingsDto } from "src/app/models/dtos/rtu/rtuNetworkSettingsDto";
import { SignalrService } from "src/app/api/signalr.service";
import { InitializeRtuDto, RtuInitializedWebDto } from "src/app/models/dtos/rtu/rtuInitializedWebDto";
import { Subscription } from "rxjs";
import { ReturnCode } from "src/app/models/enums/returnCode";
import { ReturnCodePipe } from "src/app/pipes/return-code.pipe";
import { OneApiService } from "src/app/api/one.service";
import { Utils } from "src/app/utils/utils";
import { RtuMaker } from "src/app/models/enums/rtuMaker";
import { userInfo } from "os";
import { RegistrationAnswerDto } from "src/app/models/dtos/registrationAnswerDto";
import { Role } from "src/app/models/enums/role";

@Component({
  selector: "ft-rtu-network-settings",
  templateUrl: "./ft-rtu-network-settings.component.html",
  styleUrls: ["./ft-rtu-network-settings.component.css"],
})
export class FtRtuNetworkSettingsComponent implements OnInit {
  vm: RtuNetworkSettingsDto = new RtuNetworkSettingsDto();
  public isSpinnerVisible = true;
  public isButtonDisabled = true;
  public initializationReturnCode = ReturnCode.Ok;
  public initializationMessage: string;
  allRtuMakers = RtuMaker;

  user: RegistrationAnswerDto;

  constructor(
    private activeRoute: ActivatedRoute,
    private oneApiService: OneApiService,
    private returnCodePipe: ReturnCodePipe
  ) {
    this.isSpinnerVisible = true;
  }

  async ngOnInit() {
    const id = this.activeRoute.snapshot.paramMap.get("id");
    this.vm = (await this.oneApiService
      .getRequest(`rtu/network-settings/${id}`)
      .toPromise()) as RtuNetworkSettingsDto;
    console.log("rtu network settings received");
    this.isSpinnerVisible = false;
    this.allRtuMakers = RtuMaker;
    this.user = JSON.parse(sessionStorage.getItem("currentUser"));
    this.isButtonDisabled = this.user.role > Role.WebOperator;
  }

  processInitializationResult(resultDto: RtuInitializedWebDto) {
    if (resultDto.returnCode === ReturnCode.RtuInitializedSuccessfully) {
      resultDto.rtuNetworkSettings.rtuTitle = this.vm.rtuTitle;
      this.vm = resultDto.rtuNetworkSettings;
    } else {
    }
    this.initializationMessage = this.returnCodePipe.transform(
      resultDto.returnCode
    );
    if (
      resultDto.errorMessage !== null &&
      resultDto.errorMessage !== undefined
    ) {
      this.initializationMessage += ";  " + resultDto.errorMessage;
    }
    console.log(resultDto);
    this.setStandardView(resultDto);
  }

  async initializeRtu(synch: boolean) {
    const id = this.activeRoute.snapshot.paramMap.get("id");
    this.setRequestView();

    const currentUser = JSON.parse(sessionStorage.currentUser);
    const dto = new InitializeRtuDto();
    dto.rtuId = id;
    dto.connectionId = currentUser.connectionId;
    dto.isSynchronizationRequired = synch;
    const res = (await this.oneApiService
      .postRequest(`rtu/initialize`, dto)
      .toPromise()) as RtuInitializedWebDto;
    this.processInitializationResult(res);
  }

  setStandardView(resultDto: RtuInitializedWebDto) {
    if (resultDto.returnCode === ReturnCode.Error) {
      resultDto.returnCode = ReturnCode.RtuInitializationError;
    } else {
      resultDto.returnCode = ReturnCode.RtuInitializedSuccessfully;
    }
   // window.alert(this.returnCodePipe.transform(resultDto.returnCode));
    this.isSpinnerVisible = false;
    this.isButtonDisabled = this.user.role > Role.WebOperator;

    const matCard = document.querySelector("mat-card");
    matCard.removeAttribute("id");
  }

  setRequestView() {
    this.initializationMessage = "";
    this.isSpinnerVisible = true;
    this.isButtonDisabled = true;

    const matCard = document.querySelector("mat-card");
    matCard.setAttribute("id", "card-opacity");
  }
}
