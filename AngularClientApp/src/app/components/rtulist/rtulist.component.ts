import { Component, Input } from '@angular/core';
import { RtuDto } from '../../models/rtuDto';

@Component({
  selector: 'ft-rtulist',
  templateUrl: './rtulist.component.html'
})
export class RtuListComponent {
  @Input() rtuArray: RtuDto[];

  constructor() {}
}
