import { Component, OnInit, Input } from '@angular/core';
import { TraceDto } from 'src/app/models/dtos/traceDto';

@Component({
  selector: 'ft-rtu-children',
  templateUrl: './ft-rtu-children.component.html',
  styleUrls: ['./ft-rtu-children.component.scss']
})
export class FtRtuChildrenComponent implements OnInit {
  @Input() fullPortCount: number;
  @Input() traceArray: TraceDto[];

  constructor() {}

  ngOnInit() {
    console.log(this.fullPortCount);
  }
}
