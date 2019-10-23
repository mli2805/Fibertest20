import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'ft-trace-information',
  templateUrl: './ft-trace-information.component.html',
  styleUrls: ['./ft-trace-information.component.scss']
})
export class FtTraceInformationComponent implements OnInit {
  @Input() vm: any;

  constructor() { }

  ngOnInit() {
  }

}
