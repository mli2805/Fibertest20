import { Component, Input, OnInit } from '@angular/core';
import { RtuDto } from '../../../models/rtuDto';

@Component({
  selector: 'ft-rtulist',
  templateUrl: './rtulist.component.html',
  styleUrls: ['./rtulist.component.scss']
})
export class RtuListComponent implements OnInit {
  @Input() rtuArray: RtuDto[];

  constructor() {}

  ngOnInit() {
  }
}
