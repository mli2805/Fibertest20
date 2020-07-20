import { Component, OnInit, ViewChild } from "@angular/core";
import { OneApiService } from "src/app/api/one.service";
import {
  SorAreaComponent,
  SorReader,
  SorTrace,
  SorAreaViewerService,
  SorViewerService,
  EventTableService,
  SorData,
} from "@veex/sor";
import { HttpClient } from "@angular/common/http";
import { VX_DIALOG_SERVICE } from "@veex/common";
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
    private oneApiService: OneApiService,
    private http: HttpClient
  ) {}

  async ngOnInit() {
    await this.loadFromServer();
  }

  async loadFromServer() {
    const params = JSON.parse(sessionStorage.getItem("sorFileRequestParams"));
    console.log(params);
    const sorFileId = params["sorFileId"];
    const isBaseIncluded = params["isBaseIncluded"];

    const measSorData = await this.loadSorTraceFromServer(sorFileId, false);
    this.sorTraces.push(new SorTrace(measSorData, "measurement", true));
    if (isBaseIncluded) {
      const baseSorData = await this.loadSorTraceFromServer(sorFileId, false);
      this.sorTraces.push(new SorTrace(baseSorData, "base", true));
    }

    this.sorAreaService.set(this.sorTraces);
    this.loaded = true;
  }

  async loadSorTraceFromServer(
    sorFileId: number,
    isBase: boolean
  ): Promise<SorData> {
    const blob = (await this.oneApiService.getVxSorOctetStreamFromServer(
      sorFileId,
      isBase
    )) as Blob;
    const arrayBuffer = await new Response(blob).arrayBuffer();

    const uint8arr = new Uint8Array(arrayBuffer);
    return await new SorReader().fromBytes(uint8arr);
  }
}
