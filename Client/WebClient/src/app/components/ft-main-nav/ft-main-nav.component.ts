import { Component, OnInit, OnDestroy } from "@angular/core";
import { AuthService } from "src/app/api/auth.service";
import { Subscription } from "rxjs";
import { SignalrService } from "src/app/api/signalr.service";
import { EventStatus } from "src/app/models/enums/eventStatus";
import { FiberStatePipe } from "src/app/pipes/fiber-state.pipe";
import { TraceStateDto } from "src/app/models/dtos/trace/traceStateDto";
import { UnseenOpticalService } from "src/app/interaction/unseen-optical.service";

@Component({
  selector: "ft-main-nav",
  templateUrl: "./ft-main-nav.component.html",
  styleUrls: ["./ft-main-nav.component.css"],
})
export class FtMainNavComponent implements OnInit, OnDestroy {
  private measurementAddedSubscription: Subscription;

  constructor(
    private authService: AuthService,
    private signalRService: SignalrService,
    private fiberStatePipe: FiberStatePipe,
    private unseenOpticalService: UnseenOpticalService
  ) {
    unseenOpticalService.opticalEventConfirmed$.subscribe((sorFileId) => {
      console.log(`optical event ${sorFileId} has been seen`);
    });
  }

  ngOnInit() {
    this.measurementAddedSubscription = this.signalRService.measurementAddedEmitter.subscribe(
      (signal: TraceStateDto) => {
        console.log("Measurement Added Signal received! ", signal);
        if (signal.eventStatus > EventStatus.JustMeasurementNotAnEvent) {
          alert(
            `RTU ${signal.header.rtuTitle}  Trace ${
              signal.header.traceTitle
            }  State ${this.fiberStatePipe.transform(signal.traceState)}`
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
