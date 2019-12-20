import { Component, OnInit, Input } from "@angular/core";
import { RtuDto } from "src/app/models/dtos/rtuTree/rtuDto";
import { RtuApiService } from "src/app/api/rtu.service";
import { UserDto } from "src/app/models/dtos/userDto";

@Component({
  selector: "ft-about",
  templateUrl: "./ft-about.component.html",
  styleUrls: ["./ft-about.component.css"]
})
export class FtAboutComponent implements OnInit {
  rtus: RtuDto[];
  loggedUser: UserDto;
  columnsToDisplay = ["title", "version", "version2"];

  constructor(private rtuService: RtuApiService) {}

  ngOnInit() {
    this.loggedUser = JSON.parse(sessionStorage.getItem("currentUser"));
    this.rtuService.getAllRtu().subscribe((res: RtuDto[]) => {
      console.log("rtu tree received");
      this.rtus = res;
    });
  }
}
