import { Component, OnInit } from "@angular/core";
import { RtuApiService } from "src/app/api/rtu.service";
import { ActivatedRoute } from "@angular/router";
import { RtuNetworkSettingsDto } from "src/app/models/dtos/rtuNetworkSettingsDto";

@Component({
  selector: "ft-rtu-network-settings",
  templateUrl: "./ft-rtu-network-settings.component.html",
  styleUrls: ["./ft-rtu-network-settings.component.css"]
})
export class FtRtuNetworkSettingsComponent implements OnInit {
  vm: RtuNetworkSettingsDto;

  constructor(
    private activeRoute: ActivatedRoute,
    private rtuApiService: RtuApiService
  ) {}

  ngOnInit() {
    const id = this.activeRoute.snapshot.paramMap.get("id");
    this.rtuApiService
      .getOneRtu(id, "network-settings")
      .subscribe((res: RtuNetworkSettingsDto) => {
        console.log("rtu network settings received");
        this.vm = res;
      });
  }
}
