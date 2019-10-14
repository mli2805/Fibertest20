import { Component, OnInit, Input } from '@angular/core';
import { RtuDto } from 'src/app/models/dtos/rtuDto';

@Component({
  selector: 'ft-rtu-tree',
  templateUrl: './ft-rtu-tree.component.html',
  styleUrls: ['./ft-rtu-tree.component.scss']
})
export class FtRtuTreeComponent implements OnInit {
  @Input() rtuArray: RtuDto[];

  constructor() {}

  ngOnInit() {
  }
}
