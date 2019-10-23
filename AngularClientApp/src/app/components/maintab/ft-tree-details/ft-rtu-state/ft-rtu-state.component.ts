import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'ft-rtu-state',
  templateUrl: './ft-rtu-state.component.html',
  styleUrls: ['./ft-rtu-state.component.scss']
})
export class FtRtuStateComponent implements OnInit {
  @Input() vm: any;
  constructor() { }

  ngOnInit() {
  }

}
