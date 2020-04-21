import { Component, OnInit } from "@angular/core";
import { TraceApiService } from "src/app/api/trace.service";
import { FtComponentDataProvider } from "src/app/providers/ft-component-data-provider";
import { TraceDto } from "src/app/models/dtos/rtuTree/traceDto";
import { AssignBaseParamsDto } from "src/app/models/dtos/trace/assignBaseParamsDto";
import { TranslateService } from "@ngx-translate/core";

@Component({
  selector: "ft-assign-base",
  templateUrl: "./ft-assign-base.component.html",
  styleUrls: ["./ft-assign-base.component.css"],
})
export class FtAssignBaseComponent implements OnInit {
  message;
  isSpinnerVisible = false;
  isButtonDisabled = false;

  trace: TraceDto = new TraceDto();
  params: AssignBaseParamsDto = new AssignBaseParamsDto();
  rtuTitle;
  traceTitle;
  tracePort;

  preciseRef;
  fastRef;
  additionalRef;

  constructor(
    private dataStorage: FtComponentDataProvider,
    private traceApiService: TraceApiService,
    private ts: TranslateService
  ) {}

  ngOnInit() {
    this.trace = this.dataStorage.data["trace"];
    this.traceApiService
      .getRequest("assign-base-params", this.trace.traceId)
      .subscribe((res: AssignBaseParamsDto) => {
        this.params = res;
        this.fillTheForm(res);
      });
  }

  fillTheForm(res: AssignBaseParamsDto) {
    this.traceTitle = this.trace.title;
    this.rtuTitle = res.rtuTitle;
    this.tracePort =
      this.trace.port > 0
        ? this.trace.otauPort.isPortOnMainCharon
          ? this.trace.port
          : `${this.trace.otauPort.serial}-${this.trace.port}`
        : this.ts.instant("SID_not_attached");
    this.preciseRef = res.hasPrecise ? this.ts.instant("SID_Saved_in_DB") : "";
    this.fastRef = res.hasFast ? this.ts.instant("SID_Saved_in_DB") : "";
    this.additionalRef = res.hasAdditional
      ? this.ts.instant("SID_Saved_in_DB")
      : "";
  }

  preciseChanged(fileInputEvent: any) {
    this.preciseRef = fileInputEvent.target.files[0].name;
  }

  fastChanged(fileInputEvent: any) {
    this.fastRef = fileInputEvent.target.files[0].name;
  }

  additionalChanged(fileInputEvent: any) {
    this.additionalRef = fileInputEvent.target.files[0].name;
  }

  preciseCleaned() {
    this.preciseRef = "";
  }
  fastCleaned() {
    this.fastRef = "";
  }
  additionalCleaned() {
    this.additionalRef = "";
  }
}
