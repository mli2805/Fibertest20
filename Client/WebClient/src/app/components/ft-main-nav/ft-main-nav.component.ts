import { Component, OnInit, OnDestroy } from "@angular/core";
import { AuthService } from "src/app/api/auth.service";
import { Subscription } from "rxjs";
import { SignalrService } from "src/app/api/signalr.service";
import { EventStatus } from "src/app/models/enums/eventStatus";
import { TraceStateDto } from "src/app/models/dtos/trace/traceStateDto";
import { UnseenOpticalService } from "src/app/interaction/unseen-optical.service";
import { Dictionary } from "src/app/utils/dictionary";
import { FiberState } from "src/app/models/enums/fiberState";

@Component({
  selector: "ft-main-nav",
  templateUrl: "./ft-main-nav.component.html",
  styleUrls: ["./ft-main-nav.component.css"],
})
export class FtMainNavComponent implements OnInit, OnDestroy {
  private measurementAddedSubscription: Subscription;
  private unseenOpticalsDict;

  public isOpticalAlarmVisible = false;

  constructor(
    private authService: AuthService,
    private signalRService: SignalrService,
    unseenOpticalService: UnseenOpticalService
  ) {
    this.unseenOpticalsDict = new Dictionary<string, number>();
    unseenOpticalService.opticalEventConfirmed$.subscribe((sorFileId) => {
      console.log(`optical event ${sorFileId} has been seen`);
      this.unseenOpticalsDict.removeByValue(sorFileId);
      this.isOpticalAlarmVisible = this.unseenOpticalsDict.count() > 0;
      console.log(
        `unseen optical events dict contains now ${this.unseenOpticalsDict.count()} entries`
      );
    });
  }

  ngOnInit() {
    this.measurementAddedSubscription = this.signalRService.measurementAddedEmitter.subscribe(
      (signal: TraceStateDto) => {
        console.log("Measurement Added Signal received! ", signal);
        if (signal.eventStatus > EventStatus.JustMeasurementNotAnEvent) {
          if (signal.traceState === FiberState.Ok) {
            this.unseenOpticalsDict.remove(signal.traceId);
          } else {
            this.unseenOpticalsDict.addOrUpdate(
              signal.traceId,
              signal.sorFileId
            );
          }
          this.isOpticalAlarmVisible = this.unseenOpticalsDict.count() > 0;
          console.log(
            `unseen optical events dict contains now ${this.unseenOpticalsDict.count()} entries`
          );
        }
      }
    );
  }

  ngOnDestroy(): void {
    this.measurementAddedSubscription.unsubscribe();
  }

  logout() {
    this.authService.logout().subscribe(() => {
      console.log("logout sent.");
      sessionStorage.removeItem("currentUser");
    });
  }
}
