import { Component, Input } from '@angular/core';
import { RtuDto } from '../../../models/rtuDto';

@Component({
  selector: 'ft-rtulist',
  templateUrl: './rtulist.component.html',
  styleUrls: ['./rtulist.component.scss']
})
export class RtuListComponent {
  @Input() rtuArray: RtuDto[];

  constructor() {}
}
