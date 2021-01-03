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
  public withoutEmptyNodes: boolean;

  private requestResult: TraceLandmarksDto;

  displayedColumns = [
    "ordinal",
    "nodeTitle",
    "eqType",
    "equipmentTitle",
    "distanceKm",
    "eventOrdinal",
    "coors",
  ];

  gpsFormats: GpsFormat[] = [
    { value: 0, viewValue: "ddd.dddddd\xB0" },
    { value: 1, viewValue: "ddd\xB0 mm.mmmm'" },
    { value: 2, viewValue: "ddd\xB0 mm' ss.ss\"" },
  ];
  public currentGpsFormat = 0;

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
        this.requestResult = res;
        this.applyRequestResultToView();
      });
  }

  applyRequestResultToView() {
    this.vm.header = this.requestResult.header;
    if (this.withoutEmptyNodes) {
      this.vm.landmarks = this.requestResult.landmarks.filter(
        (l) => l.eventOrdinal !== -1
      );
    } else {
      this.vm.landmarks = this.requestResult.landmarks;
    }
  }

  changedSlider() {
    this.applyRequestResultToView();
  }

  getEventNumberForTable(eventOrdinal: number) {
    if (eventOrdinal === -1) {
      return this.ts.instant("SID_no");
    }
    return eventOrdinal;
  }

  getCoorsForTable(coors: GeoPoint) {
    return this.toDetailedString(coors, this.currentGpsFormat);
  }

  toDetailedString(geoPoint: GeoPoint, mode: number): string {
    switch (mode) {
      case 0:
        return `${geoPoint.latitude.toFixed(
          6
        )}\xB0  ${geoPoint.longitude.toFixed(6)}\xB0`;
      case 1: {
        const dLat = Math.trunc(geoPoint.latitude);
        const mLat = (geoPoint.latitude - dLat) * 60;

        const dLng = Math.trunc(geoPoint.longitude);
        const mLng = (geoPoint.longitude - dLng) * 60;
        return `${dLat}\xB0${mLat
          .toFixed(4)
          .padStart(7, "0")}\'  ${dLng}\xB0${mLng
          .toFixed(4)
          .padStart(7, "0")}\'`;
      }
      case 2: {
        const dLat = Math.trunc(geoPoint.latitude);
        const mLat = (geoPoint.latitude - dLat) * 60;
        const miLat = Math.trunc(mLat);
        const sLat = (mLat - miLat) * 60;

        const dLng = Math.trunc(geoPoint.longitude);
        const mLng = (geoPoint.longitude - dLng) * 60;
        const miLng = Math.trunc(mLng);
        const sLng = (mLng - miLng) * 60;
        return `${dLat}\xB0${miLat.toString().padStart(2, "0")}\'${sLat
          .toFixed(2)
          .padStart(5, "0")}\" ${dLng}\xB0${miLng
          .toString()
          .padStart(2, "0")}\'${sLng.toFixed(2).padStart(5, "0")}\"`;
      }
      default:
        return "";
    }
  }
}

interface GpsFormat {
  value: number;
  viewValue: string;
}
