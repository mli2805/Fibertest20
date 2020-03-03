import { Component, OnInit, OnDestroy } from "@angular/core";
import { RtuApiService } from "src/app/api/rtu.service";
import { ActivatedRoute } from "@angular/router";
import { RtuNetworkSettingsDto } from "src/app/models/dtos/rtu/rtuNetworkSettingsDto";
import { SignalrService } from "src/app/api/signalr.service";
import { RtuInitializedWebDto } from "src/app/models/dtos/rtu/rtuInitializedWebDto";
import { Subscription } from "rxjs";
import { ReturnCode } from "src/app/models/enums/returnCode";
import { ReturnCodePipe } from "src/app/pipes/return-code.pipe";

@Component({
  selector: "ft-rtu-network-settings",
  templateUrl: "./ft-rtu-network-settings.component.html",
  styleUrls: ["./ft-rtu-network-settings.component.css"]
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
    private rtuApiService: RtuApiService,
    private signalRService: SignalrService,
    private returnCodePipe: ReturnCodePipe
  ) {
    this.isSpinnerVisible = true;
    this.isButtonDisabled = false;
  }

  ngOnInit() {
    const id = this.activeRoute.snapshot.paramMap.get("id");
    this.rtuApiService
      .getOneRtu(id, "network-settings")
      .subscribe((res: RtuNetworkSettingsDto) => {
        console.log("rtu network settings received");
        this.vm = res;
        this.isSpinnerVisible = false;
      });

    this.subscription = this.signalRService.rtuInitializedEmitter.subscribe(
      (signal: RtuInitializedWebDto) => {
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
        this.standardView();
        console.log(signal);
      }
    );
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  initializeRtu() {
    const id = this.activeRoute.snapshot.paramMap.get("id");
    this.whileRequestView();
    this.signalRService.initializeRtu(id);
  }

  standardView() {
    this.isSpinnerVisible = false;
    this.isButtonDisabled = false;

    const matCard = document.querySelector("mat-card");
    matCard.removeAttribute("id");
  }

  whileRequestView() {
    this.initializationMessage = "";
    this.isSpinnerVisible = true;
    this.isButtonDisabled = true;

    const matCard = document.querySelector("mat-card");
    matCard.setAttribute("id", "card-opacity");
  }
}
