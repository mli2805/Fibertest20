import { Component, OnInit } from '@angular/core';
import { RtuApiService } from 'src/app/api/rtu-api.service';
import { RtuDto } from 'src/app/models/dtos/rtuDto';
import { TraceApiService } from 'src/app/api/trace-api.service';

@Component({
  selector: 'ft-maintab',
  templateUrl: './maintab.component.html',
  styleUrls: ['./maintab.component.scss']
})
export class FtMainTabComponent implements OnInit {
  private rtus: RtuDto[];

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
