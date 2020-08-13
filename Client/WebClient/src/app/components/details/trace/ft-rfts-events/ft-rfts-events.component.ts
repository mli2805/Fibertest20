import { Component, OnInit, Input } from "@angular/core";
import { TranslateService } from "@ngx-translate/core";
import {
  RftsLevelDto,
  RftsEventsDto,
} from "src/app/models/dtos/trace/rftsEventsDto";
import { OneApiService } from "src/app/api/one.service";
import { ActivatedRoute } from "@angular/router";

@Component({
  selector: "ft-rfts-events",
  templateUrl: "./ft-rfts-events.component.html",
  styleUrls: ["./ft-rfts-events.component.css"],
})
export class FtRftsEventsComponent implements OnInit {
  public vm: RftsEventsDto = new RftsEventsDto();
  public isSpinnerVisible: boolean;

  constructor(
    private activeRoute: ActivatedRoute,
    private oneApiService: OneApiService,
    private ts: TranslateService
  ) {}

  async ngOnInit() {
    this.isSpinnerVisible = true;

    const sorFileId = this.activeRoute.snapshot.paramMap.get("id");
    await this.getData(sorFileId);

    this.isSpinnerVisible = false;
  }

  async getData(sorFileId: string) {
    await this.oneApiService
      .getRequest(`sor/rfts-events/${sorFileId}`)
      .toPromise()
      .then((res) => {
        console.log(res);
        this.vm = res as RftsEventsDto;
      })
      .catch((err) => {
        console.log(err);
      });
  }
}
