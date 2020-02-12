import { Component } from "@angular/core";
import { AuthService } from "src/app/api/auth.service";

@Component({
  selector: "ft-main-nav",
  templateUrl: "./ft-main-nav.component.html",
  styleUrls: ["./ft-main-nav.component.css"]
})
export class FtMainNavComponent {
  constructor(private authService: AuthService) {}

  logout() {
    console.log("logout pressed...");
    this.authService.logout().subscribe(() => {
      console.log("logout service run");
    });
  }
}
