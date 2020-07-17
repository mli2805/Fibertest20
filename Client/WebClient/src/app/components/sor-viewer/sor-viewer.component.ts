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
import { FileData, VX_DIALOG_SERVICE } from "@veex/common";
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
    // const params = JSON.parse(sessionStorage.getItem("sorFileRequestParams"));
    // console.log(params);
    // const sorFileId = params["sorFileId"];
    // const isBaseIncluded = params["isBaseIncluded"];

    // const bytes = await this.oneApiService.getSorFileFromServer(sorFileId);
    // if (bytes !== null) {
    //   console.log(`now we are going to show ref from ${bytes.length} bytes`);
    // }

    await this.loadSor();
  }

  async loadSor() {
    console.log("111");
    const sorFileName = "merged.vxsor";
    const arrayBuffer = await this.getSamplesFile(sorFileName);
    const fileData = new FileData(sorFileName, new Uint8Array(arrayBuffer));
    const sorData = await new SorReader().fromFileData(fileData);
    this.sorTrace = new SorTrace(sorData, fileData.nameWithoutExtention, false);

    this.sorAreaService.set([this.sorTrace]);
    this.loaded = true;
    console.log("222");
  }

  private async getSamplesFile(fileName: string): Promise<ArrayBuffer> {
    const url = `/assets/samples/${fileName}`;
    const result = await this.http
      .get(url, {
        observe: "response",
        responseType: "arraybuffer",
      })
      .toPromise();
    return result.body;
  }
}
