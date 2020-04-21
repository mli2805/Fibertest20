import { Component, OnInit } from "@angular/core";

@Component({
  selector: "ft-assign-base",
  templateUrl: "./ft-assign-base.component.html",
  styleUrls: ["./ft-assign-base.component.css"],
})
export class FtAssignBaseComponent implements OnInit {
  message;
  isSpinnerVisible = false;
  isButtonDisabled = false;

  preciseRef = "Saved in Db";
  fastRef = "not set";
  additionalRef = "not set";

  traceTitle = "my little precious trace";

  constructor() {}

  ngOnInit() {}

  preciseChanged(fileInputEvent: any) {
    this.preciseRef = fileInputEvent.target.files[0].name;
  }

  fastChanged(fileInputEvent: any) {
    this.fastRef = fileInputEvent.target.files[0].name;
  }

  additionalChanged(fileInputEvent: any) {
    this.additionalRef = fileInputEvent.target.files[0].name;
  }

  preciseCleaned() {
    this.preciseRef = "not set";
  }
  fastCleaned() {
    this.fastRef = "not set";
  }
  additionalCleaned() {
    this.additionalRef = "not set";
  }
}
