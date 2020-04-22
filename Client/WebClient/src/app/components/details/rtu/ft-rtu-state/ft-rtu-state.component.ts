import { Component, OnInit } from "@angular/core";
import { RtuStateDto } from "src/app/models/dtos/rtu/rtuStateDto";
import { ActivatedRoute } from "@angular/router";
import { OneApiService } from "src/app/api/one.service";

@Component({
  selector: "ft-rtu-state",
  templateUrl: "./ft-rtu-state.component.html",
  styleUrls: ["./ft-rtu-state.component.css"],
})
export class FtRtuStateComponent implements OnInit {
  vm: RtuStateDto = new RtuStateDto();

  displayedColumns = [
    "port",
    "traceTitle",
    "traceState",
    "lastMeasId",
    "lastMeasTime",
  ];

  constructor(
    private activeRoute: ActivatedRoute,
    private oneApiService: OneApiService
  ) {}

  ngOnInit() {
    const id = this.activeRoute.snapshot.paramMap.get("id");
    this.oneApiService
      .getRequest(`rtu/state/${id}`)
      .subscribe((res: RtuStateDto) => {
        console.log("rtu state received");
        Object.assign(this.vm, res);
      });
  }
}
