import { Component, OnInit, ViewChild } from "@angular/core";
import { OneApiService } from "src/app/api/one.service";
import {
  SorAreaComponent,
  SorReader,
  SorTrace,
  SorAreaViewerService,
  SorViewerService,
  EventTableService,
} from "@veex/sor";
import { VX_DIALOG_SERVICE, Color } from "@veex/common";
import { ChartDataService, ChartMatrixesService } from "@veex/chart";
import { DialogService } from "./other/DialogService";

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
    private oneApiService: OneApiService
  ) {}

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

    const measSorTrace = await this.loadSorTraceFromServer(
      isSorFile,
      sorFileId,
      measGuid,
      false
    );
    measSorTrace.chart.color = Color.fromRgb(0, 0, 255);
    measSorTrace.chart.name = "measurement";
    this.sorTraces.push(measSorTrace);
    if (isBaseIncluded) {
      const baseSorTrace = await this.loadSorTraceFromServer(
        isSorFile,
        sorFileId,
        measGuid,
        true
      );
      baseSorTrace.chart.color = Color.fromRgb(0, 255, 0);
      baseSorTrace.chart.name = "base";
      this.sorTraces.push(baseSorTrace);

      // this.sorViewerService.setTracesOffset(0);
      this.sorViewerService.showTracesOffset = false;
    }

    this.sorAreaService.set(this.sorTraces);
    this.loaded = true;
  }

  async loadSorTraceFromServer(
    isSorFile: boolean,
    sorFileId: number,
    measGuid: string,
    isBase: boolean
  ): Promise<SorTrace> {
    const blob = (await this.oneApiService.getSorAsBlobFromServer(
      isSorFile,
      sorFileId,
      measGuid,
      isBase,
      true
    )) as Blob;
    const arrayBuffer = await new Response(blob).arrayBuffer();

    const uint8arr = new Uint8Array(arrayBuffer);
    const sorData = await new SorReader().fromBytes(uint8arr);
    return new SorTrace(sorData, "", true);
  }
}
