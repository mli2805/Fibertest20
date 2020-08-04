import { CanActivate, Router } from "@angular/router";
import { Injectable } from "@angular/core";

@Injectable()
export class AuthGuard implements CanActivate {
  constructor(private router: Router) {}

  async canActivate() {
    if (sessionStorage.getItem("currentUser") != null) {
      return true;
    }

    console.log("Current user data not found in session storage.");
    this.router.navigate(["/login"]);
    return false;
  }
}
