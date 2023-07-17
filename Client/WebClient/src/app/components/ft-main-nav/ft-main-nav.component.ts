import {
  Component,
  OnInit,
  OnDestroy,
  ViewChild,
  ElementRef,
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
import { OneApiService } from "src/app/api/one.service";
import { ReturnCode } from "src/app/models/enums/returnCode";
import { NavigationStart, Router } from "@angular/router";
import {
  FtMessageBox,
  MessageBoxButton,
  MessageBoxStyle,
} from "../ft-simple-dialog/ft-message-box";
import { MatDialog } from "@angular/material";
import { ServerAsksClientToExitDto } from "src/app/models/dtos/serverAsksClientToExitDto";
import { ClientMeasurementDoneDto } from "src/app/models/dtos/port/clientMeasurementDoneDto";
import { SorFileManager } from "src/app/utils/sorFileManager";
import { Utils } from "src/app/utils/utils";
import { HeartbeatSender } from "src/app/utils/heartbeatSender";
import { TraceTachDto } from "src/app/models/dtos/trace/traceTachDto";
import { RtuStateAlarmIndicator } from "src/app/models/dtos/alarms/rtuStateAlarm";
import { StateAccidentDto } from "src/app/models/dtos/stateAccidentDto";

@Component({
  selector: "ft-main-nav",
  templateUrl: "./ft-main-nav.component.html",
  styleUrls: ["./ft-main-nav.component.css"],
})
export class FtMainNavComponent implements OnInit, OnDestroy {
  @ViewChild("outletDiv", { static: false })
  outletDiv: ElementRef<HTMLDivElement>;
  private heartbeatAskedSubscription: Subscription;
  private measurementAddedSubscription: Subscription;
  private stateAccidentReceivedSubscription: Subscription;
  private traceTachedSubscription: Subscription;
  private networkEventAddedSubscription: Subscription;
  private bopEventAddedSubscription: Subscription;
  private serverAsksExitSubscription: Subscription;
  private measEmmitterSubscription: Subscription;

  private opticalAlarmIndicator: OpticalAlarmIndicator;
  private networkAlarmIndicator: NetworkAlarmIndicator;
  private bopAlarmIndicator: BopAlarmIndicator;
  private rtuStateAlarmIndicator: RtuStateAlarmIndicator;

  public isOpticalAlarm = "";
  public isNetworkAlarm = "";
  public isBopAlarm = "";
  public isRtuStateAlarm = "";
  public version = "2.1.1.531";

  private language: string;
  private timer;

  constructor(
    private router: Router,
    private authService: AuthService,
    private oneApiService: OneApiService,
    private signalRService: SignalrService,
    private alarmsService: AlarmsService,
    private ts: TranslateService,
    private matDialog: MatDialog
  ) {
    console.log("main nav c-tor");

    if (sessionStorage.getItem("currentUser") !== null) {
      signalRService.fullStartProcedure();
    }

    this.language = sessionStorage.getItem("language");

    this.opticalAlarmIndicator = new OpticalAlarmIndicator(
      "currentOpticalAlarms"
    );
    this.networkAlarmIndicator = new NetworkAlarmIndicator(
      "currentNetworkAlarms"
    );
    this.bopAlarmIndicator = new BopAlarmIndicator("currentBopAlarms");
    this.rtuStateAlarmIndicator = new RtuStateAlarmIndicator("currentRtuStateAlarms");

    this.initializeIndicators();
    this.alarmsService.initialAlarmsCame$.subscribe(() =>
      this.initializeIndicators()
    );

    this.subscribeSignalRNotifications();
    this.subscribeUserSeenAlarms();

    router.events.subscribe((event) => {
      if (event instanceof NavigationStart) {
        console.log(`route ${router.url} -> ${event.url}`);
        if (router.url === "/ft-main-nav/rtu-tree") {
          const pos = this.outletDiv.nativeElement.scrollTop;
          sessionStorage.setItem("scrollTop", pos.toString());
          console.log(`position when leaving was ${pos}`);
        }
        if (event.url === "/ft-main-nav/rtu-tree") {
          const pos = sessionStorage.getItem("scrollTop");
          this.outletDiv.nativeElement.scrollTop = +pos;
          console.log(`position when coming back is ${pos}`);
        }
      }
    });
  }

  private flag = -1;
  async ngOnInit() {
    this.timer = setInterval(() => {
      try {
        this.doSend();
      } catch {
        console.log(`exception while heartbeat`);
      }
    }, 7000);
  }

  async doSend() {
    const res = await HeartbeatSender.Send(this.oneApiService);
    if (res === -9) {
      await this.exit();
      return;
    } else if (this.flag < res) {
      const settings = JSON.parse(sessionStorage.settings);
      this.version = settings.version;
      this.flag = res;
    }
  }

  async exit() {
    this.clearSessionStorage();
    this.initializeIndicators();
    this.signalRService.stopConnection();

    FtMessageBox.showAndGoAlong(
      this.matDialog,
      this.ts.instant("SID_Server_connection_lost_"),
      this.ts.instant("SID_Error_"),
      this.ts.instant("SID_Application_closed"),
      MessageBoxButton.Ok,
      false,
      MessageBoxStyle.Full,
      "600px"
    );

    this.router.navigate(["/ft-main-nav/logout"]);
  }

  async initializeIndicators() {
    console.log("initializeIndicators");
    if (sessionStorage.getItem("currentOpticalAlarms") !== null) {
      console.log("from sessionStorage");
      this.isNetworkAlarm = this.networkAlarmIndicator.GetIndicator();
      this.isOpticalAlarm = this.opticalAlarmIndicator.GetIndicator();
      this.isBopAlarm = this.bopAlarmIndicator.GetIndicator();
      this.isRtuStateAlarm = this.rtuStateAlarmIndicator.GetIndicator();
    } else {
      this.isOpticalAlarm = "";
      this.isNetworkAlarm = "";
      this.isBopAlarm = "";
      this.isRtuStateAlarm = "";
    }
  }

  private subscribeUserSeenAlarms() {
    this.alarmsService.opticalEventConfirmed$.subscribe((sorFileId) => {
      console.log(`optical event ${sorFileId} has been seen`);
      this.isOpticalAlarm = this.opticalAlarmIndicator.MarkAlarmHasBeenSeen(
        sorFileId
      );
    });
    this.alarmsService.networkEventConfirmed$.subscribe((eventId) => {
      console.log(`network event ${eventId} has been seen`);
      this.isNetworkAlarm = this.networkAlarmIndicator.MarkAlarmHasBeenSeen(
        eventId
      );
    });
    this.alarmsService.bopEventConfirmed$.subscribe((eventId) => {
      this.isBopAlarm = this.bopAlarmIndicator.MarkAlarmHasBeenSeen(eventId);
    });
    this.alarmsService.rtuStateAccidentConfirmed$.subscribe((accidentId) => {
      console.log(`rtu state accident ${accidentId} has been seen`);
      this.isRtuStateAlarm = this.rtuStateAlarmIndicator.MarkAlarmHasBeenSeen(
        accidentId
      );
    });
  }

  private subscribeSignalRNotifications() {
    this.heartbeatAskedSubscription = this.signalRService.serverAsksHeartbeatEmitter.subscribe(
      () => {
        this.doSend();
      }
    );
    this.measurementAddedSubscription = this.signalRService.measurementAddedEmitter.subscribe(
      (signal: TraceStateDto) => this.onMeasurementAdded(signal)
    );
    this.stateAccidentReceivedSubscription = this.signalRService.stateAccidentEmitter.subscribe(
      (signal: StateAccidentDto) => this.onStateAccidentReceived(signal)
    );
    this.traceTachedSubscription = this.signalRService.traceTachEmitter.subscribe(
      (signal: TraceTachDto) => this.onTraceTached(signal)
    );
    this.networkEventAddedSubscription = this.signalRService.networkEventAddedEmitter.subscribe(
      (signal: NetworkEventDto) => this.onNetworkEventAdded(signal)
    );

    this.bopEventAddedSubscription = this.signalRService.bopEventAddedEmitter.subscribe(
      (signal: BopEventDto) => this.onBopEventAdded(signal)
    );

    this.serverAsksExitSubscription = this.signalRService.serverAsksExitEmitter.subscribe(
      (signal: ServerAsksClientToExitDto) => this.onServerAsksExit(signal)
    );

    this.measEmmitterSubscription = this.signalRService.clientMeasEmitter.subscribe(
      (signal: ClientMeasurementDoneDto) => this.onMeasurementClientDone(signal)
    );
  }

  async onMeasurementClientDone(signal: ClientMeasurementDoneDto) {
    console.log(signal);
    const currentUser = JSON.parse(sessionStorage.currentUser);
    if (signal.connectionId !== currentUser.connectionId) {
      console.log(`It is measurement for another web client`);
      return;
    }

    if (signal.returnCode === ReturnCode.MeasurementEndedNormally) {
      console.log(
        `Measurement (Client) done. Request bytes for id ${signal.clientMeasurementId}`
      );
      SorFileManager.Show(
        this.router,
        false,
        0,
        signal.clientMeasurementId,
        false,
        "meas",
        new Date(),
        "",
      );
    }
  }

  async onServerAsksExit(signal: ServerAsksClientToExitDto) {
    console.log(signal);
    const res = JSON.parse(sessionStorage.getItem("currentUser"));
    if (signal.connectionId === res.connectionId) {
      await this.logout();
      await FtMessageBox.showAndGoAlong(
        this.matDialog,
        this.ts.instant(
          "SID_User__0__is_logged_in_from_a_different_device_at__1_",
          { 0: res.username, 1: new Date().toLocaleString() }
        ),
        this.ts.instant("SID_Attention_"),
        this.ts.instant("SID_Application_closed"),
        MessageBoxButton.Ok,
        false,
        MessageBoxStyle.Full,
        "600px"
      );
      this.router.navigate(["/ft-main-nav/logout"]);
    }
  }

  onMeasurementAdded(signal: TraceStateDto) {
    if (signal.eventStatus > EventStatus.JustMeasurementNotAnEvent) {
      this.isOpticalAlarm = this.opticalAlarmIndicator.TraceStateChanged(
        signal
      );
    }
  }

  onStateAccidentReceived(signal: StateAccidentDto) {
    this.isRtuStateAlarm =
      this.rtuStateAlarmIndicator.RtuStateAccidentReceived(signal);
  }

  onTraceTached(signal: TraceTachDto) {
    console.log(`traceTached ${signal}`);
    this.isOpticalAlarm = this.opticalAlarmIndicator.TraceTached(signal);
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
    this.traceTachedSubscription.unsubscribe();
    this.measurementAddedSubscription.unsubscribe();
    this.networkEventAddedSubscription.unsubscribe();
    this.bopEventAddedSubscription.unsubscribe();
    this.serverAsksExitSubscription.unsubscribe();
    this.measEmmitterSubscription.unsubscribe();
    this.heartbeatAskedSubscription.unsubscribe();
  }

  async logout() {
    try {
      this.clearSessionStorage();
      this.initializeIndicators();
      this.signalRService.stopConnection();
      await this.authService.logout().toPromise();
    } catch {
      console.log(`exception while logging out`);
    }
  }

  clearSessionStorage() {
    sessionStorage.removeItem("currentUser");
    sessionStorage.removeItem("currentOpticalAlarms");
    this.opticalAlarmIndicator.ClearList();
    sessionStorage.removeItem("currentNetworkAlarms");
    this.networkAlarmIndicator.ClearList();
    sessionStorage.removeItem("currentBopAlarms");
    this.bopAlarmIndicator.ClearList();
    sessionStorage.removeItem("currentRtuStateAlarms");
    this.rtuStateAlarmIndicator.ClearList();
    console.log("session storage cleaned.");
  }

  toggleLanguage() {
    if (this.language === "ru") {
      this.language = "en";
    } else {
      this.language = "ru";
    }
    this.ts.use(this.language);
    sessionStorage.setItem("language", this.language);

    location.reload(); // quite slow
  }

  showHelpPdf() {
    window.open("../../../assets/UserGuide/FIBERTEST20ClientUGru.pdf#page=81");
  }
}
