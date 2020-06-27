import { Component, OnInit, OnDestroy } from "@angular/core";
import { AuthService } from "src/app/api/auth.service";
import { Subscription } from "rxjs";
import { SignalrService } from "src/app/api/signalr.service";
import { EventStatus } from "src/app/models/enums/eventStatus";
import { TraceStateDto } from "src/app/models/dtos/trace/traceStateDto";
import { Dictionary } from "src/app/utils/dictionary";
import { FiberState } from "src/app/models/enums/fiberState";
import { AlarmsService } from "src/app/interaction/alarms.service";
import { NetworkEventDto } from "src/app/models/dtos/networkEventDto";
import { ChannelEvent } from "src/app/models/enums/channelEvent";
import {
  OpticalAlarm,
  OpticalAlarmIndicator,
} from "src/app/models/dtos/alarms/opticalAlarm";
import { NetworkAlarmIndicator } from "src/app/models/dtos/alarms/networkAlarm";
import { AlarmsDto } from "src/app/models/dtos/alarms/alarmsDto";
import { BopAlarmIndicator } from "src/app/models/dtos/alarms/bopAlarm";

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
  private bopAlarmIndicator: BopAlarmIndicator;

  public isOpticalAlarm = "";
  public isNetworkAlarm = "";
  public isBopAlarm = "";

  constructor(
    private authService: AuthService,
    private signalRService: SignalrService,
    private alarmsService: AlarmsService
  ) {
    this.opticalAlarmIndicator = new OpticalAlarmIndicator();
    this.networkAlarmIndicator = new NetworkAlarmIndicator();
    this.bopAlarmIndicator = new BopAlarmIndicator();

    this.alarmsService.initialAlarmsCame$.subscribe((json) =>
      this.initializeIndicators(json)
    );
  }

  async initializeIndicators(json: string) {
    console.log("main-nav received alarms");
    const alarmsDto = JSON.parse(json) as AlarmsDto;
    alarmsDto.networkAlarms.forEach((na) => {
      this.networkAlarmIndicator.list.push(na);
    });
    alarmsDto.opticalAlarms.forEach((oa) =>
      this.opticalAlarmIndicator.list.push(oa)
    );
    console.log(alarmsDto);
    this.isNetworkAlarm = this.networkAlarmIndicator.GetIndicator();
    this.isOpticalAlarm = this.opticalAlarmIndicator.GetIndicator();
    this.isBopAlarm = this.bopAlarmIndicator.GetIndicator();
    this.subscribeNewAlarmEvents();
    this.subscribeUserSeenAlarms();
  }

  ngOnInit() {}

  private subscribeUserSeenAlarms() {
    this.alarmsService.opticalEventConfirmed$.subscribe((sorFileId) => {
      console.log(`optical event ${sorFileId} has been seen`);
      this.isOpticalAlarm = this.opticalAlarmIndicator.AlarmHasBeenSeen(
        sorFileId
      );
      console.log(
        `unseen optical events: ${this.opticalAlarmIndicator.list.length}`
      );
    });
    this.alarmsService.networkEventConfirmed$.subscribe((eventId) => {
      console.log(`network event ${eventId} has been seen`);
      this.isNetworkAlarm = this.networkAlarmIndicator.AlarmHasBeenSeen(
        eventId
      );
      console.log(
        `unseen network events: ${this.networkAlarmIndicator.list.length}`
      );
    });
  }

  private subscribeNewAlarmEvents() {
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
