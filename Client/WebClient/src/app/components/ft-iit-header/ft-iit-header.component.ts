import { Component, OnInit } from "@angular/core";

@Component({
  selector: "ft-iit-header",
  templateUrl: "./ft-iit-header.component.html",
  styleUrls: ["./ft-iit-header.component.css"],
})
export class FtIitHeaderComponent implements OnInit {
  version: string;

  constructor() {}

  ngOnInit() {
    if (sessionStorage.settings === undefined) {
      this.version = ``;
    }
    const settings = JSON.parse(sessionStorage.settings);
    this.version = settings.version;
  }
}
