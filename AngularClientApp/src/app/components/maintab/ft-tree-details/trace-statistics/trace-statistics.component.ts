import { Component, OnInit, Input } from '@angular/core';
import { TraceStatisticsDto } from 'src/app/models/dtos/traceStatisticsDto';

@Component({
  selector: 'ft-trace-statistics',
  templateUrl: './trace-statistics.component.html',
  styleUrls: ['./trace-statistics.component.scss']
})
export class FtTraceStatisticsComponent implements OnInit {
  @Input() vm: TraceStatisticsDto;

  columnsToDisplayBaseRefs = ['baseRefType', 'assignmentTimestamp', 'username'];
  columnsToDisplay = [
    'sorFileId',
    'baseRefType',
    'eventRegistrationTimestamp',
    'isEvent',
    'traceState'
  ];

  constructor() {}

  ngOnInit() {}
}
