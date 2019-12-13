import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { RtuApiService } from "src/app/api/rtu.service";
import { RtuInformationDto } from "src/app/models/dtos/rtu/rtuInformationDto";

@Component({
  selector: "ft-rtu-information",
  templateUrl: "./ft-rtu-information.component.html",
  styleUrls: ["./ft-rtu-information.component.css"]
})
export class FtRtuInformationComponent implements OnInit {
  private vm: RtuInformationDto = new RtuInformationDto();

  constructor(
    private activeRoute: ActivatedRoute,
    private rtuApiService: RtuApiService
  ) {}

  ngOnInit() {
    const id = this.activeRoute.snapshot.paramMap.get("id");
    this.rtuApiService
      .getOneRtu(id, "information")
      .subscribe((res: RtuInformationDto) => {
        console.log("rtu information received");
        this.vm = res;
      });
  }
}
