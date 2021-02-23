import { Component, OnInit } from "@angular/core";
import { environment } from "src/environments/environment";

@Component({
  selector: "ft-iit-header",
  templateUrl: "./ft-iit-header.component.html",
  styleUrls: ["./ft-iit-header.component.css"],
})
export class FtIitHeaderComponent implements OnInit {
  version: string;

  constructor() {}

  ngOnInit() {
    if (environment.production === true) {
      this.version = "2.0";
    } else {
      if (sessionStorage.settings === undefined) {
        this.version = `There is no SETTINGS in sessionStorage!!!`;
      }
      const settings = JSON.parse(sessionStorage.settings);
      this.version = settings.version;
    }
  }
}
