import { Component, OnInit } from "@angular/core";
import { AuthService } from "src/app/api/auth.service";
import { UserDto } from "src/app/models/dtos/userDto";
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

@Component({
  selector: "ft-login",
  templateUrl: "./ft-login.component.html",
  styleUrls: ["./ft-login.component.css"],
})
export class FtLoginComponent implements OnInit {
  resultMessage: string;
  public isSpinnerVisible = false;

  constructor(
    private router: Router,
    private authService: AuthService,
    private oneApiService: OneApiService,
    private signalrService: SignalrService,
    private unseenAlarmService: AlarmsService,
    private httpClient: HttpClient,

    private returnCodePipe: ReturnCodePipe
  ) {
    this.isSpinnerVisible = false;

    this.getSettings().subscribe((settings) => {
      console.log("Settings are: ", settings);
      sessionStorage.setItem("settings", JSON.stringify(settings));
    });
  }

  user: string;
  pw: string;

  ngOnInit() {}

  public getSettings(): Observable<any> {
    return this.httpClient.get("./assets/settings.json");
  }

  async login() {
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

    try {
      const res = (await this.authService
        .login(this.user, this.pw)
        .toPromise()) as UserDto;
      if (res === null) {
        console.log("Login failed, try again...");
      } else {
        sessionStorage.setItem("currentUser", JSON.stringify(res));

        this.signalrService.buildConnection(res.jsonWebToken);
        this.signalrService.startConnection();
        console.log("Logged in successfully!");

        const alarms = (await this.oneApiService
          .getRequest("misc/alarms", null)
          .toPromise()) as AlarmsDto;
        const json = JSON.stringify(alarms);
        this.unseenAlarmService.processInitialAlarms(json);
        this.router.navigate(["/rtu-tree"], { queryParams: null });
      }
    } catch (unsuccessfulResult) {
      if (unsuccessfulResult.error.returnCode === undefined) {
        this.resultMessage = this.returnCodePipe.transform(
          ReturnCode.C2DWcfConnectionError
        );
      } else {
        this.resultMessage = this.returnCodePipe.transform(
          unsuccessfulResult.error.returnCode
        );
      }
      console.log("login: " + unsuccessfulResult.error);
    }

    this.isSpinnerVisible = false;
  }
}
