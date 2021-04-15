import { Component, OnInit } from "@angular/core";
import { AuthService } from "src/app/api/auth.service";
import { RegistrationAnswerDto } from "src/app/models/dtos/registrationAnswerDto";
import { environment } from "src/environments/environment";
import { Router } from "@angular/router";
import { ReturnCodePipe } from "src/app/pipes/return-code.pipe";
import { ReturnCode } from "src/app/models/enums/returnCode";
import { SignalrService } from "src/app/api/signalr.service";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { OneApiService } from "src/app/api/one.service";
import { AlarmsDto } from "src/app/models/dtos/alarms/alarmsDto";
import { AlarmsService } from "src/app/interaction/alarms.service";
import { Utils } from "src/app/utils/utils";
import { TranslateService } from "@ngx-translate/core";
import {
  FtMessageBox,
  MessageBoxButton,
  MessageBoxStyle,
} from "../ft-simple-dialog/ft-message-box";
import { MatDialog } from "@angular/material";

@Component({
  selector: "ft-login",
  templateUrl: "./ft-login.component.html",
  styleUrls: ["./ft-login.component.css"],
})
export class FtLoginComponent implements OnInit {
  user: string;
  pw: string;
  resultMessage: string;
  public isSpinnerVisible = false;

  constructor(
    private router: Router,
    private authService: AuthService,
    private oneApiService: OneApiService,
    private signalrService: SignalrService,
    private unseenAlarmService: AlarmsService,
    private httpClient: HttpClient,

    private returnCodePipe: ReturnCodePipe,
    private ts: TranslateService,
    private matDialog: MatDialog
  ) {}

  async ngOnInit() {
    this.isSpinnerVisible = false;

    const settings = await this.getSettings().toPromise();
    console.log("Settings are: ", settings);
    sessionStorage.setItem("settings", JSON.stringify(settings));
  }

  private getSettings(): Observable<any> {
    const uuid = Utils.generateUUID();
    return this.httpClient.get(`./assets/settings.json?${uuid}`);
  }

  async login() {
    this.initializeVariables();

    try {
      const settings = JSON.parse(sessionStorage.settings);

      const res = (await this.authService
        .login(this.user, this.pw, settings.version)
        .toPromise()) as RegistrationAnswerDto;
      sessionStorage.setItem("currentUser", JSON.stringify(res));

      const connectionId = await this.signalrService.fullStartProcedure();

      console.log(
        `Logged in with signalR connection id ${connectionId} successfully!`
      );

      await this.initializeIndicators(res);
      this.router.navigate(["/ft-main-nav/rtu-tree"], { queryParams: null });
    } catch (unsuccessfulResult) {
      this.handleRegistrationProblems(unsuccessfulResult);
    }
    this.isSpinnerVisible = false;
  }

  initializeVariables() {
    this.resultMessage = "";
    if (
      environment.production === false &&
      this.user === undefined &&
      this.pw === undefined
    ) {
      this.user = "root";
      this.pw = "root";
    }
    this.isSpinnerVisible = true;
  }

  async handleRegistrationProblems(unsuccessfulResult) {
    if (unsuccessfulResult.error === undefined) {
      this.resultMessage = unsuccessfulResult.message;
    } else if (unsuccessfulResult.error.returnCode === undefined) {
      this.resultMessage = this.returnCodePipe.transform(
        ReturnCode.C2DWcfConnectionError
      );
    } else if (
      unsuccessfulResult.error.returnCode === ReturnCode.VersionsDoNotMatch
    ) {
      const mess = await this.closeCurrentTab();
      this.resultMessage = this.returnCodePipe.transform(
        ReturnCode.VersionsDoNotMatch
      );
      this.isSpinnerVisible = false;

      if (environment.production === false) {
        const settings = JSON.parse(sessionStorage.settings);
        settings.version = unsuccessfulResult.error.serverVersion;
        sessionStorage.setItem("settings", JSON.stringify(settings));
      }
    } else {
      this.resultMessage = this.returnCodePipe.transform(
        unsuccessfulResult.error.returnCode
      );
    }
    console.log("login: " + this.resultMessage);
  }

  async initializeIndicators(res: RegistrationAnswerDto) {
    const alarms = (await this.oneApiService
      .getRequest("misc/alarms", null)
      .toPromise()) as AlarmsDto;
    sessionStorage.setItem(
      "currentOpticalAlarms",
      JSON.stringify(alarms.opticalAlarms)
    );
    sessionStorage.setItem(
      "currentNetworkAlarms",
      JSON.stringify(alarms.networkAlarms)
    );
    sessionStorage.setItem(
      "currentBopAlarms",
      JSON.stringify(alarms.bopAlarms)
    );

    this.unseenAlarmService.processInitialAlarms();
  }

  async closeCurrentTab() {
    this.signalrService.stopConnection();

    const answer = await FtMessageBox.showAndGoAlong(
      this.matDialog,
      this.ts.instant("SID_Versions_do_not_match"),
      this.ts.instant("SID_Error_"),
      this.ts.instant("SID_Please_close_browser"),
      MessageBoxButton.Ok,
      false,
      MessageBoxStyle.Full,
      "600px"
    );
  }
}
