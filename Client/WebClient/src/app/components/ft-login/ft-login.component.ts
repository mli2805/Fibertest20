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
    private signalrService: SignalrService,
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

    this.authService.login(this.user, this.pw).subscribe(
      (res: UserDto) => {
        if (res === null) {
          console.log("Login failed, try again...");
        }
        sessionStorage.setItem("currentUser", JSON.stringify(res));

        this.signalrService.buildConnection(res.jsonWebToken);
        this.signalrService.startConnection();

        this.router.navigate(["/rtu-tree"], { queryParams: null });
        this.isSpinnerVisible = false;
      },
      (unsuccessfulResult: any) => {
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
        this.isSpinnerVisible = false;
      }
    );
  }
}
