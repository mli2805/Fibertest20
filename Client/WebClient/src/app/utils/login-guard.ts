import { CanActivate, Router } from "@angular/router";
import { Injectable } from "@angular/core";

@Injectable()
export class LoginGuard implements CanActivate {
  constructor(private router: Router) {}

  async canActivate() {
    if (sessionStorage.getItem("currentUser") == null) {
      return true;
    }

    this.router.navigate(["/rtu-tree"]);
    return false;
  }
}
