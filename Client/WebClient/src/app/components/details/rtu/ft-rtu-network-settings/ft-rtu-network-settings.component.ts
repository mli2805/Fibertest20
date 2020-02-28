import { Component, OnInit, OnDestroy } from "@angular/core";
import { RtuApiService } from "src/app/api/rtu.service";
import { ActivatedRoute } from "@angular/router";
import { RtuNetworkSettingsDto } from "src/app/models/dtos/rtu/rtuNetworkSettingsDto";
import { SignalrService } from "src/app/api/signalr.service";
import { RtuInitializedWebDto } from "src/app/models/dtos/rtu/rtuInitializedWebDto";
import { Subscription } from "rxjs";

@Component({
  selector: "ft-rtu-network-settings",
  templateUrl: "./ft-rtu-network-settings.component.html",
  styleUrls: ["./ft-rtu-network-settings.component.css"]
})
export class FtRtuNetworkSettingsComponent implements OnInit, OnDestroy {
  vm: RtuNetworkSettingsDto = new RtuNetworkSettingsDto();
  private subscription: Subscription;

  constructor(
    private activeRoute: ActivatedRoute,
    private rtuApiService: RtuApiService,
    private signalRService: SignalrService
  ) {}

  ngOnInit() {
    const id = this.activeRoute.snapshot.paramMap.get("id");
    this.rtuApiService
      .getOneRtu(id, "network-settings")
      .subscribe((res: RtuNetworkSettingsDto) => {
        console.log("rtu network settings received");
        this.vm = res;
      });

    this.subscription = this.signalRService.rtuInitializedEmitter.subscribe(
      (signal: RtuInitializedWebDto) => {
        console.log(signal);
      }
    );
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  initializeRtu() {
    const id = this.activeRoute.snapshot.paramMap.get("id");
    this.signalRService.initializeRtu(id);
  }
}
