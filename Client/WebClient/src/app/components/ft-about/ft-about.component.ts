import { Component, OnInit, Input } from "@angular/core";
import { RtuDto } from "src/app/models/dtos/rtuTree/rtuDto";
import { RtuApiService } from "src/app/api/rtu.service";
import { UserDto } from "src/app/models/dtos/userDto";
import { Role } from "src/app/models/enums/role";
import { MiscService } from "src/app/api/misc.service";
import { AboutDto, AboutRtuDto } from "src/app/models/dtos/aboutDto";

@Component({
  selector: "ft-about",
  templateUrl: "./ft-about.component.html",
  styleUrls: ["./ft-about.component.css"]
})
export class FtAboutComponent implements OnInit {
  loggedUser: UserDto;
  aboutVm: AboutDto;
  rtus: AboutRtuDto[];
  roleString: string;
  columnsToDisplay = ["title", "version", "version2"];

  constructor(
    private rtuService: RtuApiService,
    private miscService: MiscService
  ) {}

  ngOnInit() {
    this.loggedUser = JSON.parse(sessionStorage.getItem("currentUser"));
    this.roleString = Role[this.loggedUser.role];

    this.aboutVm = new AboutDto();
    this.aboutVm.dcSoftware = "asdf";
    this.aboutVm.webApiSoftware = "acbsdtw";

    this.miscService.getAbout().subscribe((res: AboutDto) => {
      console.log("about view model received");
      this.aboutVm = res;
    });
  }
}
