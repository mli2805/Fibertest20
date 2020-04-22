import { Component, OnInit, Input } from "@angular/core";
import { UserDto } from "src/app/models/dtos/userDto";
import { Role } from "src/app/models/enums/role";
import { AboutDto, AboutRtuDto } from "src/app/models/dtos/aboutDto";
import { OneApiService } from "src/app/api/one.service";

@Component({
  selector: "ft-about",
  templateUrl: "./ft-about.component.html",
  styleUrls: ["./ft-about.component.css"],
})
export class FtAboutComponent implements OnInit {
  loggedUser: UserDto;
  aboutVm: AboutDto = new AboutDto();
  rtus: AboutRtuDto[];
  roleString: string;
  columnsToDisplay = ["title", "version", "version2"];

  constructor(private oneApiService: OneApiService) {}

  ngOnInit() {
    this.loggedUser = JSON.parse(sessionStorage.getItem("currentUser"));
    this.roleString = Role[this.loggedUser.role];

    this.oneApiService.getRequest("misc/about").subscribe((res: AboutDto) => {
      this.aboutVm = res;
    });
  }
}
