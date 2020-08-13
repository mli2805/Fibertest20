import { Component, OnInit, Input } from "@angular/core";
import { RftsEventsDto } from "src/app/models/dtos/trace/rftsEventsDto";

@Component({
  selector: "ft-rfts-events-level",
  templateUrl: "./ft-rfts-events-level.component.html",
  styleUrls: ["./ft-rfts-events-level.component.css"],
})
export class FtRftsEventsLevelComponent implements OnInit {
  @Input() vm: RftsEventsDto;
  constructor() {}

  ngOnInit() {}
}
