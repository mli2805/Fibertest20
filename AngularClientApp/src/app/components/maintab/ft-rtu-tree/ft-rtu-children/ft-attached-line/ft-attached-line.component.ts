import { Component, OnInit, Input } from '@angular/core';
import { TraceDto } from 'src/app/models/dtos/traceDto';
import { TraceApiService } from 'src/app/api/trace-api.service';
import { GuidDto } from 'src/app/models/dtos/guidDto';

@Component({
  selector: 'ft-attached-line',
  templateUrl: './ft-attached-line.component.html',
  styleUrls: ['./ft-attached-line.component.scss']
})
export class FtAttachedLineComponent implements OnInit {
  @Input() trace: TraceDto;

  constructor(private traceService: TraceApiService) {}

  ngOnInit() {}

  myClickFunction(event) {
    alert('Information');
    const traceId = '2fc00df8-da04-4211-b154-0adda9b6f3ab';
    this.traceService.getTraceStatistics(traceId).subscribe(res => {
      console.log(res);
    });
  }
}
