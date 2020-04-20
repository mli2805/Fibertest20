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

  clearClicked(order: number) {
    console.log(`clear clicked ${order}`);
  }
}
