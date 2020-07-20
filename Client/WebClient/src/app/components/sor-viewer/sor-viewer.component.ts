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
  sorTrace: SorTrace = null;

  constructor(
    public sorAreaService: SorAreaViewerService,
    private oneApiService: OneApiService,
    private http: HttpClient
  ) {}

  async ngOnInit() {
    await this.loadSorFromServer();
  }

  async loadSorFromServer() {
    const params = JSON.parse(sessionStorage.getItem("sorFileRequestParams"));
    console.log(params);
    const sorFileId = params["sorFileId"];
    const isBaseIncluded = params["isBaseIncluded"];

    const blob = (await this.oneApiService.getVxSorOctetStreamFromServer(
      sorFileId
    )) as Blob;
    const arrayBuffer = await new Response(blob).arrayBuffer();

    const uint8arr = new Uint8Array(arrayBuffer);
    const sorData = await new SorReader().fromBytes(uint8arr);
    this.sorTrace = new SorTrace(sorData, "", true);

    this.sorAreaService.set([this.sorTrace]);
    this.loaded = true;
  }
}
