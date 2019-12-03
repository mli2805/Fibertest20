import { Component, OnInit } from "@angular/core";
import { LoginService } from "src/app/api/login.service";
import { UserDto } from "src/app/models/dtos/userDto";
import { environment } from "src/environments/environment";
import { Router } from "@angular/router";

@Component({
  selector: "ft-login",
  templateUrl: "./ft-login.component.html",
  styleUrls: ["./ft-login.component.css"]
})
export class FtLoginComponent implements OnInit {
  constructor(
    private router: Router,
    private loginService: LoginService // private loginInteracionService: LoginInteractionService
  ) {}

  user: string;
  pw: string;

  ngOnInit() {}

  login() {
    if (
      environment.production === false &&
      this.user === undefined &&
      this.pw === undefined
    ) {
      this.user = "root";
      this.pw = "root";
    }

    this.loginService.login(this.user, this.pw).subscribe((res: UserDto) => {
      if (res === null) {
        console.log("Login failed, try again...");
      }
      sessionStorage.setItem("currentUser", JSON.stringify(res));

      this.router.navigate(["/rtu-tree"], { queryParams: null });
    });
  }
}
