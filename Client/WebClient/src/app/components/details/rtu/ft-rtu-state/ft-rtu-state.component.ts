import { Component, OnInit } from "@angular/core";
import { RtuStateDto } from "src/app/models/dtos/rtuStateDto";
import { ActivatedRoute } from "@angular/router";
import { RtuApiService } from "src/app/api/rtu.service";

@Component({
  selector: "ft-rtu-state",
  templateUrl: "./ft-rtu-state.component.html",
  styleUrls: ["./ft-rtu-state.component.css"]
})
export class FtRtuStateComponent implements OnInit {
  vm: RtuStateDto;

  constructor(
    private activeRoute: ActivatedRoute,
    private rtuApiService: RtuApiService
  ) {}

  ngOnInit() {
    const id = this.activeRoute.snapshot.paramMap.get("id");
    this.rtuApiService.getOneRtu(id, "state").subscribe((res: RtuStateDto) => {
      console.log("rtu state received");
      this.vm = res;
    });
  }
}
