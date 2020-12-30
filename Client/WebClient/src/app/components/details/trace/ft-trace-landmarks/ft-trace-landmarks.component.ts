import { Component, Input, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { TranslateService } from "@ngx-translate/core";
import { OneApiService } from "src/app/api/one.service";
import { TraceLandmarksDto } from "src/app/models/dtos/trace/traceLandmarksDto";
import { GeoPoint } from "src/app/models/underlying/geoPoint";

@Component({
  selector: "ft-trace-landmarks",
  templateUrl: "./ft-trace-landmarks.component.html",
  styleUrls: ["./ft-trace-landmarks.component.css"],
})
export class FtTraceLandmarksComponent implements OnInit {
  @Input() vm = new TraceLandmarksDto();
  public isSpinnerVisible: boolean;

  displayedColumns = [
    "ordinal",
    "nodeTitle",
    "eqType",
    "equipmentTitle",
    "distanceKm",
    "eventOrdinal",
    "coors",
  ];

  constructor(
    private activeRoute: ActivatedRoute,
    private oneApiService: OneApiService,
    private ts: TranslateService
  ) {}

  ngOnInit() {
    this.isSpinnerVisible = true;
    const id = this.activeRoute.snapshot.paramMap.get("id");
    this.oneApiService
      .getRequest(`trace/landmarks/${id}`)
      .subscribe((res: TraceLandmarksDto) => {
        this.isSpinnerVisible = false;
        console.log(res);
        this.vm = res;
      });
  }

  getEventNumberForTable(eventOrdinal: number) {
    if (eventOrdinal === -1) {
      return this.ts.instant("SID_no");
    }
    return eventOrdinal;
  }

  getCoorsForTable(coors: GeoPoint) {
    return `${coors.latitude.toFixed(6)} ${coors.longitude.toFixed(6)}`;
  }
}
