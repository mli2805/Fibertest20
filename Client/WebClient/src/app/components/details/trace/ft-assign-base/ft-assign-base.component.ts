import { Component, OnInit } from "@angular/core";
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
import { ReturnCodePipe } from "src/app/pipes/return-code.pipe";

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
  preciseInputString;
  fastFile;
  fastInputString;
  additionalFile;
  additionalInputString;

  assignDto = new AssignBaseRefDtoWithFiles();

  constructor(
    private router: Router,
    private oneApiService: OneApiService,
    private ts: TranslateService,
    private returnCodePipe: ReturnCodePipe
  ) {
    this.savedInDb = this.ts.instant("SID_Saved_in_DB");
  }

  ngOnInit() {
    const params = JSON.parse(sessionStorage.getItem("assignBaseParams"));
    this.trace = params.trace;
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

    this.prepareDto();
    if (this.assignDto.baseRefs.length > 0) {
      this.oneApiService
        .postFile(`trace/assign-base-refs`, this.assignDto)
        .subscribe((res: RequestAnswer) => {
          console.log(res);
          if (res.returnCode === ReturnCode.BaseRefAssignedSuccessfully) {
            console.log("base refs assigned successfully!");
          } else {
            console.log("Error: ", res.errorMessage);
            this.message = this.returnCodePipe.transform(res.returnCode);
            if (res.errorMessage != null) {
              this.message += "<br/>" + res.errorMessage;
            }
            this.isSpinnerVisible = false;
            this.isButtonDisabled = false;
            return;
          }
          this.isSpinnerVisible = false;
          this.isButtonDisabled = false;
          this.router.navigate(["/rtu-tree"]);
        });
    } else {
      this.router.navigate(["/rtu-tree"]);
    }
  }

  prepareDto() {
    this.assignDto.rtuId = this.trace.rtuId;
    this.assignDto.rtuMaker = this.params.rtuMaker;
    this.assignDto.otdrId = this.params.otdrId;
    this.assignDto.traceId = this.trace.traceId;
    this.assignDto.otauPortDto = this.trace.otauPort;
    this.assignDto.baseRefs = [];
    this.processInput();
  }

  processInput() {
    this.processOneBase(
      this.preciseFile,
      this.preciseInputString,
      BaseRefType.Precise,
      this.params.preciseId
    );
    this.processOneBase(
      this.fastFile,
      this.fastInputString,
      BaseRefType.Fast,
      this.params.fastId
    );
    this.processOneBase(
      this.additionalFile,
      this.additionalInputString,
      BaseRefType.Additional,
      this.params.additionalId
    );
  }

  processOneBase(
    file: File,
    inputString: string,
    type: BaseRefType,
    previousBaseRefId: string
  ) {
    if (inputString !== this.savedInDb && inputString !== "") {
      // persist new base ref and delete old if exists
      const dto = new BaseRefFile();
      dto.type = type;
      dto.file = file;
      this.assignDto.baseRefs.push(dto);
    } else if (inputString === "" && previousBaseRefId !== this.emptyGuid) {
      // delete old base ref
      const dto = new BaseRefFile();
      dto.type = type;
      this.assignDto.baseRefs.push(dto);
    } // else - no changes - nothing to do
  }

  cancel() {
    this.router.navigate(["/ft-main-nav/rtu-tree"]);
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
    this.preciseInputString = this.setFileName(res.preciseId);
    this.fastInputString = this.setFileName(res.fastId);
    this.additionalInputString = this.setFileName(res.additionalId);
  }

  setFileName(guid: string): string {
    return guid === this.emptyGuid ? "" : this.savedInDb;
  }

  preciseChanged(fileInputEvent: any) {
    this.preciseFile = fileInputEvent.target.files[0];
    this.preciseInputString = this.preciseFile.name;
  }

  fastChanged(fileInputEvent: any) {
    this.fastFile = fileInputEvent.target.files[0];
    this.fastInputString = this.fastFile.name;
  }

  additionalChanged(fileInputEvent: any) {
    this.additionalFile = fileInputEvent.target.files[0];
    this.additionalInputString = this.additionalFile.name;
  }

  preciseCleaned() {
    this.preciseFile = undefined;
    this.preciseInputString = "";
  }
  fastCleaned() {
    this.fastFile = undefined;
    this.fastInputString = "";
  }
  additionalCleaned() {
    this.additionalFile = undefined;
    this.additionalInputString = "";
  }
}
