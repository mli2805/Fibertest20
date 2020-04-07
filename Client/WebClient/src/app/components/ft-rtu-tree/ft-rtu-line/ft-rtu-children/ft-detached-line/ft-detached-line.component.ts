import { Component, OnInit, Input } from "@angular/core";
import { TraceDto } from "src/app/models/dtos/rtuTree/traceDto";

@Component({
  selector: "ft-detached-line",
  templateUrl: "./ft-detached-line.component.html",
  styleUrls: ["./ft-detached-line.component.scss"]
})
export class FtDetachedLineComponent implements OnInit {
  @Input() trace: TraceDto;

  constructor() {}

  ngOnInit() {}
}
