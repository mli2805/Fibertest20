import { Component, OnInit } from "@angular/core";
import { AuthService } from "src/app/api/auth.service";
import { UserDto } from "src/app/models/dtos/userDto";
import { environment } from "src/environments/environment";
import { Router } from "@angular/router";
import { ReturnCodePipe } from "src/app/pipes/return-code.pipe";
import { ReturnCode } from "src/app/models/enums/returnCode";
import { SignalrService } from "src/app/api/signalr.service";

@Component({
  selector: "ft-login",
  templateUrl: "./ft-login.component.html",
  styleUrls: ["./ft-login.component.css"]
})
export class FtLoginComponent implements OnInit {
  resultMessage: string;

  constructor(
    private router: Router,
    private authService: AuthService,
    private signalrService: SignalrService,

    private returnCodePipe: ReturnCodePipe
  ) {}

  user: string;
  pw: string;

  ngOnInit() {}

  login() {
    this.resultMessage = "";
    if (
      environment.production === false &&
      this.user === undefined &&
      this.pw === undefined
    ) {
      this.user = "root";
      this.pw = "root";
    }

    this.authService.login(this.user, this.pw).subscribe(
      (res: UserDto) => {
        if (res === null) {
          console.log("Login failed, try again...");
        }
        sessionStorage.setItem("currentUser", JSON.stringify(res));

        this.signalrService.buildConnection(res.jsonWebToken);
        this.signalrService.startConnection();

        this.router.navigate(["/rtu-tree"], { queryParams: null });
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
      }
    );
  }
}
