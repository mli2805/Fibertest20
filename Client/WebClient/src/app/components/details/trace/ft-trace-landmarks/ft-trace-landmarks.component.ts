import { Component, Input, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { OneApiService } from "src/app/api/one.service";
import { TraceLandmarksDto } from "src/app/models/dtos/trace/traceLandmarksDto";

@Component({
  selector: "ft-trace-landmarks",
  templateUrl: "./ft-trace-landmarks.component.html",
  styleUrls: ["./ft-trace-landmarks.component.css"],
})
export class FtTraceLandmarksComponent implements OnInit {
  @Input() vm = new TraceLandmarksDto();
  public isSpinnerVisible: boolean;

  constructor(
    private activeRoute: ActivatedRoute,
    private oneApiService: OneApiService
  ) {}

  ngOnInit() {
    this.isSpinnerVisible = true;
    const id = this.activeRoute.snapshot.paramMap.get("id");
    this.oneApiService
      .getRequest(`trace/landmarks/${id}`)
      .subscribe((res: TraceLandmarksDto) => {
        this.isSpinnerVisible = false;
        console.log(res);
        this.vm = res;
      });
  }
}
