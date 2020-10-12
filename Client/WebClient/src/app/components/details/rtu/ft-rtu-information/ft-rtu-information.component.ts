import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { RtuInformationDto } from "src/app/models/dtos/rtu/rtuInformationDto";
import { OneApiService } from "src/app/api/one.service";

@Component({
  selector: "ft-rtu-information",
  templateUrl: "./ft-rtu-information.component.html",
  styleUrls: ["./ft-rtu-information.component.css"],
})
export class FtRtuInformationComponent implements OnInit {
  vm: RtuInformationDto = new RtuInformationDto();

  constructor(
    private activeRoute: ActivatedRoute,
    private oneApiService: OneApiService
  ) {}

  ngOnInit() {
    const id = this.activeRoute.snapshot.paramMap.get("id");
    this.oneApiService
      .getRequest(`rtu/information/${id}`)
      .subscribe((res: RtuInformationDto) => {
        console.log("rtu information received");
        this.vm = res;
      });
  }
}
