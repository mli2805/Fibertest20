import { Component, OnInit } from "@angular/core";
import { FtComponentDataProvider } from "src/app/providers/ft-component-data-provider";
import { TraceDto } from "src/app/models/dtos/rtuTree/traceDto";
import { AssignBaseParamsDto } from "src/app/models/dtos/trace/assignBaseParamsDto";
import { TranslateService } from "@ngx-translate/core";
import { Router } from "@angular/router";
import { AssignBaseRefDtoWithFiles } from "src/app/models/dtos/trace/assignBaseRefDtoWithFiles";
import { BaseRefType } from "src/app/models/enums/baseRefType";
import { OneApiService } from "src/app/api/one.service";
import { ReturnCode } from "src/app/models/enums/returnCode";
import { BaseRefFile } from "src/app/models/underlying/baseRefFile";
import { RequestAnswer } from "src/app/models/underlying/requestAnswer";

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
    this.isSpinnerVisible = true;
    this.isButtonDisabled = true;

    const dto = this.prepareDto();
    if (dto.baseRefs.length > 0) {
      console.log(dto);
      this.oneApiService
        .postFile(`trace/assign-base-refs`, dto)
        .subscribe((res: RequestAnswer) => {
          if (res.returnCode === ReturnCode.BaseRefAssignedSuccessfully) {
            console.log("base refs assigned successfully!");
          } else {
            this.message = res.errorMessage;
          }
          this.isSpinnerVisible = false;
          this.isButtonDisabled = false;
        });
    }
    this.router.navigate(["/rtu-tree"]);
  }

  prepareDto(): AssignBaseRefDtoWithFiles {
    const dto = new AssignBaseRefDtoWithFiles();
    dto.rtuId = this.trace.rtuId;
    dto.rtuMaker = this.params.rtuMaker;
    dto.otdrId = this.params.otdrId;
    dto.traceId = this.trace.traceId;
    dto.otauPortDto = this.trace.otauPort;
    dto.baseRefs = this.prepareFiles();
    return dto;
  }

  prepareFiles(): BaseRefFile[] {
    const baseRefs = [];

    if (this.isFilenameChanged(this.preciseFilename, this.params.preciseId)) {
      baseRefs.push(this.createDto(this.preciseFile, BaseRefType.Precise));
    }
    if (this.isFilenameChanged(this.fastFilename, this.params.fastId)) {
      baseRefs.push(this.createDto(this.fastFile, BaseRefType.Fast));
    }
    if (
      this.isFilenameChanged(this.additionalFilename, this.params.additionalId)
    ) {
      baseRefs.push(
        this.createDto(this.additionalFile, BaseRefType.Additional)
      );
    }

    return baseRefs;
  }

  createDto(file: File, type: BaseRefType): BaseRefFile {
    const dto = new BaseRefFile();
    dto.type = type;

    // dto without file means baseRef for that type should be deleted on server and rtu
    if (file != null) {
      dto.file = file;
    }
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
