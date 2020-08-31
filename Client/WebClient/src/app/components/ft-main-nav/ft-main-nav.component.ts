import {
  Component,
  OnInit,
  OnDestroy,
  ChangeDetectorRef,
  ApplicationRef,
} from "@angular/core";
import { AuthService } from "src/app/api/auth.service";
import { Subscription } from "rxjs";
import { SignalrService } from "src/app/api/signalr.service";
import { EventStatus } from "src/app/models/enums/eventStatus";
import { TraceStateDto } from "src/app/models/dtos/trace/traceStateDto";
import { AlarmsService } from "src/app/interaction/alarms.service";
import { NetworkEventDto } from "src/app/models/dtos/networkEventDto";
import { OpticalAlarmIndicator } from "src/app/models/dtos/alarms/opticalAlarm";
import { NetworkAlarmIndicator } from "src/app/models/dtos/alarms/networkAlarm";
import { BopAlarmIndicator } from "src/app/models/dtos/alarms/bopAlarm";
import { BopEventDto } from "src/app/models/dtos/bopEventDto";
import { TranslateService } from "@ngx-translate/core";

@Component({
  selector: "ft-main-nav",
  templateUrl: "./ft-main-nav.component.html",
  styleUrls: ["./ft-main-nav.component.css"],
})
export class FtMainNavComponent implements OnInit, OnDestroy {
  private measurementAddedSubscription: Subscription;
  private networkEventAddedSubscription: Subscription;
  private bopEventAddedSubscription: Subscription;

  private opticalAlarmIndicator: OpticalAlarmIndicator;
  private networkAlarmIndicator: NetworkAlarmIndicator;
  private bopAlarmIndicator: BopAlarmIndicator;

  public isOpticalAlarm = "";
  public isNetworkAlarm = "";
  public isBopAlarm = "";

  private language: string;

  constructor(
    private authService: AuthService,
    private signalRService: SignalrService,
    private alarmsService: AlarmsService,
    private ts: TranslateService,
    private ar: ApplicationRef,
    private cdr: ChangeDetectorRef
  ) {
    console.log("main nav c-tor hit!");
    this.language = sessionStorage.getItem("language");

    this.opticalAlarmIndicator = new OpticalAlarmIndicator(
      "currentOpticalAlarms"
    );
    this.networkAlarmIndicator = new NetworkAlarmIndicator(
      "currentNetworkAlarms"
    );
    this.bopAlarmIndicator = new BopAlarmIndicator("currentBopAlarms");

    this.initializeIndicators();
    this.signalRService.reStartConnection();
    this.alarmsService.initialAlarmsCame$.subscribe(() =>
      this.initializeIndicators()
    );

    this.subscribeNewAlarmEvents();
    this.subscribeUserSeenAlarms();
  }

  async initializeIndicators() {
    console.log("initializeIndicators");
    if (sessionStorage.getItem("currentOpticalAlarms") !== null) {
      console.log("from sessionStorage");
      this.isNetworkAlarm = this.networkAlarmIndicator.GetIndicator();
      this.isOpticalAlarm = this.opticalAlarmIndicator.GetIndicator();
      this.isBopAlarm = this.bopAlarmIndicator.GetIndicator();
    } else {
      this.isOpticalAlarm = "";
      this.isNetworkAlarm = "";
      this.isBopAlarm = "";
    }
  }

  ngOnInit() {
    setInterval(() => {}, 100);
  }

  private subscribeUserSeenAlarms() {
    this.alarmsService.opticalEventConfirmed$.subscribe((sorFileId) => {
      console.log(`optical event ${sorFileId} has been seen`);
      this.isOpticalAlarm = this.opticalAlarmIndicator.AlarmHasBeenSeen(
        sorFileId
      );
    });
    this.alarmsService.networkEventConfirmed$.subscribe((eventId) => {
      console.log(`network event ${eventId} has been seen`);
      this.isNetworkAlarm = this.networkAlarmIndicator.AlarmHasBeenSeen(
        eventId
      );
    });
    this.alarmsService.bopEventConfirmed$.subscribe((eventId) => {
      this.isBopAlarm = this.bopAlarmIndicator.AlarmHasBeenSeen(eventId);
    });
  }

  private subscribeNewAlarmEvents() {
    this.measurementAddedSubscription = this.signalRService.measurementAddedEmitter.subscribe(
      (signal: TraceStateDto) => this.onMeasurementAdded(signal)
    );
    this.networkEventAddedSubscription = this.signalRService.networkEventAddedEmitter.subscribe(
      (signal: NetworkEventDto) => this.onNetworkEventAdded(signal)
    );

    this.bopEventAddedSubscription = this.signalRService.bopEventAddedEmitter.subscribe(
      (signal: BopEventDto) => this.onBopEventAdded(signal)
    );
  }

  onMeasurementAdded(signal: TraceStateDto) {
    if (signal.eventStatus > EventStatus.JustMeasurementNotAnEvent) {
      this.isOpticalAlarm = this.opticalAlarmIndicator.TraceStateChanged(
        signal
      );
    }
  }

  onNetworkEventAdded(signal: NetworkEventDto) {
    this.isNetworkAlarm = this.networkAlarmIndicator.NetworkEventReceived(
      signal
    );
  }

  onBopEventAdded(signal: BopEventDto) {
    this.isBopAlarm = this.bopAlarmIndicator.BopEventReceived(signal);
  }

  ngOnDestroy(): void {
    this.measurementAddedSubscription.unsubscribe();
    this.networkEventAddedSubscription.unsubscribe();
    this.bopEventAddedSubscription.unsubscribe();
  }

  async logout() {
    await this.authService.logout().toPromise();
    this.signalRService.stopConnection();
    sessionStorage.removeItem("currentUser");
    sessionStorage.removeItem("currentOpticalAlarms");
    this.opticalAlarmIndicator.ClearList();
    sessionStorage.removeItem("currentNetworkAlarms");
    this.networkAlarmIndicator.ClearList();
    sessionStorage.removeItem("currentBopAlarms");
    this.bopAlarmIndicator.ClearList();
    console.log("session storage cleaned.");
    this.initializeIndicators();
  }

  toggleLanguage() {
    if (this.language === "ru") {
      this.language = "en";
    } else {
      this.language = "ru";
    }
    this.ts.use(this.language);
    sessionStorage.setItem("language", this.language);
    // this.cdr.detectChanges();
    // this.ar.tick(); // doesn't work

    location.reload(); // too slow
  }

  showHelpPdf() {
    window.open("../../../assets/UserGuide/FIBERTEST20ClientUGru.pdf#page=81");
  }
}
