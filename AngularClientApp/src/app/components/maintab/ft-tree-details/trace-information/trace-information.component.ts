import { Component, OnInit, Input } from '@angular/core';
import { TraceInformationDto } from 'src/app/models/dtos/traceInformationDto';

@Component({
  selector: 'ft-trace-information',
  templateUrl: './trace-information.component.html',
  styleUrls: ['./trace-information.component.scss']
})
export class FtTraceInformationComponent implements OnInit {
  @Input() vm: TraceInformationDto;

  constructor() { }

  ngOnInit() {
  }

}
