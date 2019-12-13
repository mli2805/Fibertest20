import { Component, OnInit, Input } from "@angular/core";
import { TraceInformationDto } from "src/app/models/dtos/trace/traceInformationDto";

@Component({
  selector: "ft-trace-information",
  templateUrl: "./ft-trace-information.component.html",
  styleUrls: ["./ft-trace-information.component.css"]
})
export class FtTraceInformationComponent implements OnInit {
  @Input() vm: TraceInformationDto;

  constructor() { }

  ngOnInit() {
  }

}
