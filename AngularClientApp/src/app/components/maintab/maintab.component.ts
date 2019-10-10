import { Component, OnInit } from '@angular/core';
import { RtuApiService } from 'src/app/api/rtu-api.service';
import { RtuDto } from 'src/app/models/dtos/rtuDto';
import { TraceDto } from 'src/app/models/dtos/traceDto';
import { TraceApiService } from 'src/app/api/trace-api.service';
import { RtuVm } from 'src/app/models/viewModels/rtuVm';

@Component({
  selector: 'ft-maintab',
  templateUrl: './maintab.component.html',
  styleUrls: ['./maintab.component.scss']
})
export class FtMainTabComponent implements OnInit {
  private rtus: RtuDto[];
  private traces: TraceDto[];

  private rtuVms: RtuVm[];

  private selectedTab: number;
  constructor(
    private rtuService: RtuApiService,
    private traceService: TraceApiService
  ) {}

  ngOnInit() {
    this.rtuService.getAllRtu().subscribe((res: RtuDto[]) => {
      this.rtus = res;
    });

    this.selectedTab = 1;
  }
}
