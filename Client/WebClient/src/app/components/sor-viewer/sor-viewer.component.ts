import { Component, OnInit, ViewChild } from "@angular/core";
import { OneApiService } from "src/app/api/one.service";
import {
  SorAreaComponent,
  SorReader,
  SorTrace,
  SorAreaViewerService,
  SorViewerService,
  EventTableService,
  SorAreaSettings,
} from "@veex/sor";
import { VX_DIALOG_SERVICE, Color } from "@veex/common";
import { ChartDataService, ChartMatrixesService } from "@veex/chart";
import { DialogService } from "./other/DialogService";
import { GetSorDataParams } from "src/app/models/dtos/meas-params/getSorDataParams";
import { OccupyRtuDto, RtuOccupation, RtuOccupationState } from "src/app/models/dtos/meas-params/occupyRtuDto";
import { RequestAnswer } from "src/app/models/underlying/requestAnswer";
import { ReturnCodePipe } from "src/app/pipes/return-code.pipe";

@Component({
  selector: "ft-sor-viewer",
  templateUrl: "./sor-viewer.component.html",
  styleUrls: ["./sor-viewer.component.css"],
  providers: [
    SorAreaViewerService,
    SorViewerService,
    EventTableService,
    ChartDataService,
    ChartMatrixesService,
    { provide: VX_DIALOG_SERVICE, useExisting: DialogService },
  ],
})
export class SorViewerComponent implements OnInit {
  @ViewChild("sorAreaComponent", { static: false })
  private sorAreaComponent: SorAreaComponent;
  loaded = false;
  measSorTrace: SorTrace = null;
  baseSorTrace: SorTrace = null;
  sorTraces: SorTrace[] = [];

  constructor(
    public sorAreaService: SorAreaViewerService,
    public sorViewerService: SorViewerService,
    private oneApiService: OneApiService,
    private returnCodePipe: ReturnCodePipe
  ) {
    sorAreaService.showLandmarksDock = true;
    const sorSettings = SorAreaSettings.Default();
    sorSettings.showDock = false;
    sorAreaService.setSettings(sorSettings);
  }

  async ngOnInit() {
    await this.loadFromServer();
  }

  async loadFromServer() {
    const params = JSON.parse(sessionStorage.getItem("sorFileRequestParams"));
    console.log(params);
    const isSorFile = JSON.parse(params["isSorFile"]);
    const sorFileId = params["sorFileId"];
    const measGuid = params["measGuid"];
    const isBaseIncluded = params["isBaseIncluded"];
    const rtuGuid = params["rtuGuid"];
    console.log(`found rtuGuid: ${rtuGuid}`);
    console.log(`found measGuid: ${measGuid}`);

    const measSorTrace = await this.loadSorTraceFromServer(
      isSorFile,
      sorFileId,
      measGuid,
      false,
      rtuGuid,
    );
    measSorTrace.chart.color = Color.fromRgb(0, 0, 255);
    measSorTrace.chart.name = params["filename"] + ".sor";
    this.sorTraces.push(measSorTrace);
    if (isBaseIncluded) {
      const baseSorTrace = await this.loadSorTraceFromServer(
        isSorFile,
        sorFileId,
        measGuid,
        true,
        rtuGuid,
      );
      baseSorTrace.chart.color = Color.fromRgb(0, 255, 0);
      baseSorTrace.chart.name = params["filename"] + " base.sor";
      this.sorTraces.push(baseSorTrace);

      this.sorViewerService.showTracesOffset = false;
      this.sorAreaService.settings.showEventTableComments = true;
    }

    this.sorAreaService.set(this.sorTraces);

    document.title = params["filename"] + ".sor";

    this.loaded = true;

    var freeDto = new OccupyRtuDto();
    freeDto.rtuId = rtuGuid;
    freeDto.state = new RtuOccupationState();
    freeDto.state.rtuId = rtuGuid;
    freeDto.state.rtuOccupation = RtuOccupation.None;

    const res = (await this.oneApiService
      .postRequest("rtu/set-rtu-occupation-state", freeDto)
      .toPromise()) as RequestAnswer;
    console.log(`${this.returnCodePipe.transform(res.returnCode)}`);
  }

  async loadSorTraceFromServer(
    isSorFile: boolean,
    sorFileId: number,
    measGuid: string,
    isBase: boolean,
    rtuGuid: string,
  ): Promise<SorTrace> {
    console.log(`start loading sor ${measGuid}  from server`);

    const params = new GetSorDataParams();
    params.isSorFileOrMeasurementClient = isSorFile.toString();
    params.sorFileId = sorFileId.toString();
    params.measGuid = measGuid;
    params.isBase = isBase.toString();
    params.isForDisplayOrSaveFormat = true.toString();
    params.rtuGuid = rtuGuid;

    const blob = (await this.oneApiService
      .getSorAsBlobFromServer(
      isSorFile,
      sorFileId,
      measGuid,
      isBase,
      true,
      rtuGuid,
    )) as Blob;

    const arrayBuffer = await new Response(blob).arrayBuffer();

    const uint8arr = new Uint8Array(arrayBuffer);
    const sorData = await new SorReader().fromBytes(uint8arr);
    console.log(`sor fetched successfully`);
    return new SorTrace(sorData, "", true);
  }
}
