import { Component, OnInit } from "@angular/core";
import { FtComponentDataProvider } from "src/app/providers/ft-component-data-provider";
import { TraceDto } from "src/app/models/dtos/rtuTree/traceDto";
import { AssignBaseParamsDto } from "src/app/models/dtos/trace/assignBaseParamsDto";
import { TranslateService } from "@ngx-translate/core";
import { Router } from "@angular/router";
import { AssignBaseReftsDto } from "src/app/models/dtos/trace/assignBaseRefsDto";
import { BaseRefDto } from "src/app/models/underlying/baseRefDto";
import { BaseRefType } from "src/app/models/enums/baseRefType";
import { OneApiService } from "src/app/api/one.service";

@Component({
  selector: "ft-assign-base",
  templateUrl: "./ft-assign-base.component.html",
  styleUrls: ["./ft-assign-base.component.css"],
})
export class FtAssignBaseComponent implements OnInit {
  savedInDb;
  emptyGuid = "00000000-0000-0000-0000-000000000000";

  message;
  isSpinnerVisible = false;
  isButtonDisabled = false;

  trace: TraceDto = new TraceDto();
  params: AssignBaseParamsDto = new AssignBaseParamsDto();
  rtuTitle;
  traceTitle;
  tracePort;

  preciseFile;
  preciseFilename;
  fastFile;
  fastFilename;
  additionalFile;
  additionalFilename;

  constructor(
    private router: Router,
    private dataStorage: FtComponentDataProvider,
    private oneApiService: OneApiService,
    private ts: TranslateService
  ) {
    this.savedInDb = this.ts.instant("SID_Saved_in_DB");
  }

  ngOnInit() {
    this.trace = this.dataStorage.data["trace"];
    this.oneApiService
      .getRequest(`trace/assign-base-params/${this.trace.traceId}`)
      .subscribe((res: AssignBaseParamsDto) => {
        this.params = res;
        this.fillTheForm(res);
      });
  }

  save() {
    const dto = this.prepareDto();
    if (dto.baseRefs.length > 0) {
      // TODO: send dto
    }
    this.router.navigate(["/rtu-tree"]);
  }

  prepareDto(): AssignBaseReftsDto {
    const dto = new AssignBaseReftsDto();
    dto.rtuId = this.trace.rtuId;
    dto.rtuMaker = this.params.rtuMaker;
    dto.otdrId = this.params.otdrId;
    dto.traceId = this.trace.traceId;
    dto.otauPortDto = this.trace.otauPort;
    dto.baseRefs = [];

    if (this.isFilenameChanged(this.preciseFilename, this.params.preciseId)) {
      dto.baseRefs.push(this.createDto(this.preciseFile, BaseRefType.Precise));
    }
    if (this.isFilenameChanged(this.fastFilename, this.params.fastId)) {
      dto.baseRefs.push(this.createDto(this.fastFile, BaseRefType.Fast));
    }
    if (
      this.isFilenameChanged(this.additionalFilename, this.params.additionalId)
    ) {
      dto.baseRefs.push(
        this.createDto(this.additionalFile, BaseRefType.Additional)
      );
    }
    return dto;
  }

  createDto(file: File, type: BaseRefType): BaseRefDto {
    const dto = new BaseRefDto();
    dto.baseRefType = type;
    if (file != null) {
      const reader = new FileReader();
      reader.onload = () => {
        dto.sorBytes = reader.result;
      };
      reader.readAsArrayBuffer(file);
    }
    console.log(dto);
    return dto;
  }

  cancel() {
    this.router.navigate(["/rtu-tree"]);
  }

  fillTheForm(res: AssignBaseParamsDto) {
    this.traceTitle = this.trace.title;
    this.rtuTitle = this.params.rtuTitle;
    this.tracePort =
      this.trace.port > 0
        ? this.trace.otauPort.isPortOnMainCharon
          ? this.trace.port
          : `${this.trace.otauPort.serial}-${this.trace.port}`
        : this.ts.instant("SID_not_attached");
    this.preciseFilename = this.setFileName(res.preciseId);
    this.fastFilename = this.setFileName(res.fastId);
    this.additionalFilename = this.setFileName(res.additionalId);
  }

  setFileName(guid: string): string {
    return guid === this.emptyGuid ? "" : this.savedInDb;
  }
  isFilenameChanged(filename: string, previousBaseRefId: string) {
    return (
      (filename !== "" && filename !== this.savedInDb) ||
      (filename === "" && previousBaseRefId !== this.emptyGuid)
    );
  }

  preciseChanged(fileInputEvent: any) {
    this.preciseFile = fileInputEvent.target.files[0];
    this.preciseFilename = this.preciseFile.name;
  }

  fastChanged(fileInputEvent: any) {
    this.fastFile = fileInputEvent.target.files[0];
    this.fastFilename = this.fastFile.name;
  }

  additionalChanged(fileInputEvent: any) {
    this.additionalFile = fileInputEvent.target.files[0];
    this.additionalFilename = this.additionalFile.name;
  }

  preciseCleaned() {
    this.preciseFilename = "";
  }
  fastCleaned() {
    this.fastFilename = "";
  }
  additionalCleaned() {
    this.additionalFilename = "";
  }
}
