import { Component, OnInit, OnDestroy } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { RtuNetworkSettingsDto } from "src/app/models/dtos/rtu/rtuNetworkSettingsDto";
import { SignalrService } from "src/app/api/signalr.service";
import { RtuInitializedWebDto } from "src/app/models/dtos/rtu/rtuInitializedWebDto";
import { Subscription } from "rxjs";
import { ReturnCode } from "src/app/models/enums/returnCode";
import { ReturnCodePipe } from "src/app/pipes/return-code.pipe";
import { OneApiService } from "src/app/api/one.service";
import { InitializeRtuDto } from "src/app/models/dtos/rtu/initializeRtuDto";

@Component({
  selector: "ft-rtu-network-settings",
  templateUrl: "./ft-rtu-network-settings.component.html",
  styleUrls: ["./ft-rtu-network-settings.component.css"],
})
export class FtRtuNetworkSettingsComponent implements OnInit, OnDestroy {
  vm: RtuNetworkSettingsDto = new RtuNetworkSettingsDto();
  private subscription: Subscription;
  public isSpinnerVisible = true;
  public isButtonDisabled = true;
  public initializationReturnCode = ReturnCode.Ok;
  public initializationMessage: string;

  constructor(
    private activeRoute: ActivatedRoute,
    private oneApiService: OneApiService,
    private signalRService: SignalrService,
    private returnCodePipe: ReturnCodePipe
  ) {
    this.isSpinnerVisible = true;
    this.isButtonDisabled = false;
  }

  ngOnInit() {
    const id = this.activeRoute.snapshot.paramMap.get("id");
    this.oneApiService
      .getRequest(`rtu/network-settings/${id}`)
      .subscribe((res: RtuNetworkSettingsDto) => {
        console.log("rtu network settings received");
        this.vm = res;
        this.isSpinnerVisible = false;
      });

    this.subscription = this.signalRService.rtuInitializedEmitter.subscribe(
      (signal: RtuInitializedWebDto) => this.processInitializationResult(signal)
    );
  }

  processInitializationResult(signal: RtuInitializedWebDto) {
    if (signal.returnCode === ReturnCode.RtuInitializedSuccessfully) {
      signal.rtuNetworkSettings.rtuTitle = this.vm.rtuTitle;
      this.vm = signal.rtuNetworkSettings;
    } else {
    }
    this.initializationMessage = this.returnCodePipe.transform(
      signal.returnCode
    );
    if (signal.errorMessage !== null && signal.errorMessage !== undefined) {
      this.initializationMessage += ";  " + signal.errorMessage;
    }
    this.setStandardView();
    console.log(signal);
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  _initializeRtu() {
    const id = this.activeRoute.snapshot.paramMap.get("id");
    this.setRequestView();
    this.signalRService.initializeRtu(id);
  }

  initializeRtu() {
    const id = this.activeRoute.snapshot.paramMap.get("id");
    this.setRequestView();
    this.oneApiService
      .postRequest(`rtu/initialize/${id}`, null)
      .subscribe((res: RtuInitializedWebDto) =>
        this.processInitializationResult(res)
      );
  }

  setStandardView() {
    this.isSpinnerVisible = false;
    this.isButtonDisabled = false;

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
