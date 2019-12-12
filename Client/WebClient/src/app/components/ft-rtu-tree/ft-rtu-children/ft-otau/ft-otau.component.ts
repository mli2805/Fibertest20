import { Component, OnInit, Input } from "@angular/core";
import { OtauWebDto } from "src/app/models/dtos/otauWebDto";

@Component({
  selector: "ft-otau",
  templateUrl: "./ft-otau.component.html",
  styleUrls: ["./ft-otau.component.css"]
})
export class FtOtauComponent implements OnInit {
  @Input() otau: OtauWebDto;

  constructor() {}

  ngOnInit() {}

  expand() {
    console.log("expand otau clicked");
    this.otau.expanded = !this.otau.expanded;
  }
}
