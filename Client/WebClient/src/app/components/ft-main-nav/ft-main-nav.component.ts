import { Component, OnInit, OnDestroy } from "@angular/core";
import { AuthService } from "src/app/api/auth.service";
import { Subscription } from "rxjs";
import { SignalrService } from "src/app/api/signalr.service";
import { EventStatus } from "src/app/models/enums/eventStatus";
import { TraceStateDto } from "src/app/models/dtos/trace/traceStateDto";
import { Dictionary } from "src/app/utils/dictionary";
import { FiberState } from "src/app/models/enums/fiberState";
import { UnseenAlarmsService } from "src/app/interaction/unseen-alarms.service";
import { NetworkEventDto } from "src/app/models/dtos/networkEventDto";
import { ChannelEvent } from "src/app/models/enums/channelEvent";
import {
  OpticalAlarm,
  OpticalAlarmIndicator,
} from "src/app/models/opticalAlarm";
import { NetworkAlarmIndicator } from "src/app/models/networkAlarm";

@Component({
  selector: "ft-main-nav",
  templateUrl: "./ft-main-nav.component.html",
  styleUrls: ["./ft-main-nav.component.css"],
})
export class FtMainNavComponent implements OnInit, OnDestroy {
  private measurementAddedSubscription: Subscription;
  private networkEventAddedSubscription: Subscription;
  private opticalAlarmIndicator: OpticalAlarmIndicator;
  private networkAlarmIndicator: NetworkAlarmIndicator;

  public isOpticalAlarm = "ok";
  public isNetworkAlarm = "ok";
  public isBopAlarm = "ok";

  constructor(
    private authService: AuthService,
    private signalRService: SignalrService,
    unseenAlarmsService: UnseenAlarmsService
  ) {
    this.opticalAlarmIndicator = new OpticalAlarmIndicator();
    this.networkAlarmIndicator = new NetworkAlarmIndicator();

    unseenAlarmsService.opticalEventConfirmed$.subscribe((sorFileId) => {
      console.log(`optical event ${sorFileId} has been seen`);
      this.isOpticalAlarm = this.opticalAlarmIndicator.AlarmHasBeenSeen(
        sorFileId
      );
      console.log(
        `unseen optical events: ${this.opticalAlarmIndicator.list.length}`
      );
    });
    unseenAlarmsService.networkEventConfirmed$.subscribe((eventId) => {
      console.log(`network event ${eventId} has been seen`);
      this.isNetworkAlarm = this.networkAlarmIndicator.AlarmHasBeenSeen(
        eventId
      );
      console.log(
        `unseen network events: ${this.networkAlarmIndicator.list.length}`
      );
    });
  }

  ngOnInit() {
    this.measurementAddedSubscription = this.signalRService.measurementAddedEmitter.subscribe(
      (signal: TraceStateDto) => this.onMeasurementAdded(signal)
    );
    this.networkEventAddedSubscription = this.signalRService.networkEventAddedEmitter.subscribe(
      (signal: NetworkEventDto) => this.onNetworkEventAdded(signal)
    );
  }

  onMeasurementAdded(signal: TraceStateDto) {
    console.log("Measurement Added Signal received! ", signal);
    if (signal.eventStatus > EventStatus.JustMeasurementNotAnEvent) {
      this.isOpticalAlarm = this.opticalAlarmIndicator.TraceStateChanged(
        signal
      );
      console.log(
        `unseen optical events: ${this.opticalAlarmIndicator.list.length}`
      );
    }
  }

  onNetworkEventAdded(signal: NetworkEventDto) {
    console.log("Network Event Added Signal received! ", signal);
    this.isNetworkAlarm = this.networkAlarmIndicator.NetworkEventReceived(
      signal
    );
    console.log(
      `unseen network events: ${this.networkAlarmIndicator.list.length}`
    );
  }

  ngOnDestroy(): void {
    this.measurementAddedSubscription.unsubscribe();
    this.networkEventAddedSubscription.unsubscribe();
  }

  logout() {
    this.authService.logout().subscribe(() => {
      console.log("logout sent.");
      sessionStorage.removeItem("currentUser");
    });
  }
}
