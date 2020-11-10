import { Component, OnInit, Input } from "@angular/core";
import { UserDto } from "src/app/models/dtos/userDto";
import { Role } from "src/app/models/enums/role";
import { AboutDto } from "src/app/models/dtos/aboutDto";
import { OneApiService } from "src/app/api/one.service";

@Component({
  selector: "ft-about",
  templateUrl: "./ft-about.component.html",
  styleUrls: ["./ft-about.component.css"],
})
export class FtAboutComponent implements OnInit {
  loggedUser: UserDto;
  aboutVm: AboutDto = new AboutDto();
  roleString: string;
  columnsToDisplay = ["title", "version", "version2"];

  constructor(private oneApiService: OneApiService) {}

  async ngOnInit() {
    this.loggedUser = JSON.parse(sessionStorage.getItem("currentUser"));
    this.roleString = Role[this.loggedUser.role];

    this.aboutVm = (await this.oneApiService
      .getRequest("misc/about")
      .toPromise()) as AboutDto;

    console.log(`WebAPI version is ${this.aboutVm.webApiSoftware}`);
    const settings = JSON.parse(sessionStorage.getItem("settings"));
    this.aboutVm.webClientSoftware = settings.version;
  }
}
