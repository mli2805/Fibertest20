import { Component, OnInit, Input } from "@angular/core";
import { OtauWebDto } from "src/app/models/dtos/otauWebDto";

@Component({
  selector: "ft-otau-v2",
  templateUrl: "./ft-otau-v2.component.html",
  styleUrls: ["./ft-otau-v2.component.css"]
})
export class FtOtauV2Component implements OnInit {
  @Input() otau: OtauWebDto;

  constructor() {}

  ngOnInit() {}

  expand() {
    console.log("expand otau clicked");
    this.otau.expanded = !this.otau.expanded;
  }
}
