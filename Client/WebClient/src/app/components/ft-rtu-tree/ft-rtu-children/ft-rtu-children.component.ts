import { Component, OnInit, Input } from "@angular/core";
import { ChildDto } from "src/app/models/dtos/childDto";

@Component({
  selector: "ft-rtu-children",
  templateUrl: "./ft-rtu-children.component.html",
  styleUrls: ["./ft-rtu-children.component.scss"]
})
export class FtRtuChildrenComponent implements OnInit {
  @Input() childArray: ChildDto[];

  constructor() {}

  ngOnInit() {}
}
