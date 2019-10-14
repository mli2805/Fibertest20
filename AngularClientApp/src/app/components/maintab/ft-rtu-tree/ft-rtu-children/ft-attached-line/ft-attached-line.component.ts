import { Component, OnInit, Input } from '@angular/core';
import { TraceDto } from 'src/app/models/dtos/traceDto';

@Component({
  selector: 'ft-attached-line',
  templateUrl: './ft-attached-line.component.html',
  styleUrls: ['./ft-attached-line.component.scss']
})
export class FtAttachedLineComponent implements OnInit {
  @Input() trace: TraceDto;

  constructor() {}

  ngOnInit() {}
}
