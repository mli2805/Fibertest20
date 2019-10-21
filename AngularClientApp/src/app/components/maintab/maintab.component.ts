import { Component, OnInit } from '@angular/core';
import { RtuApiService } from 'src/app/api/rtu-api.service';
import { RtuDto } from 'src/app/models/dtos/rtuDto';
import { TraceApiService } from 'src/app/api/trace-api.service';
import { ChildType } from 'src/app/models/enums/childType';
import { TraceDto } from 'src/app/models/dtos/traceDto';
import { OtauWebDto } from 'src/app/models/dtos/otauWebDto';

@Component({
  selector: 'ft-maintab',
  templateUrl: './maintab.component.html',
  styleUrls: ['./maintab.component.scss']
})
export class FtMainTabComponent implements OnInit {
  private rtus: RtuDto[];

  private selectedTab: number;
  constructor(private rtuService: RtuApiService) {}

  ngOnInit() {
    this.rtuService.getAllRtu().subscribe((res: RtuDto[]) => {
      this.rtus = res;
      this.applyRtuMonitoringModeToTraces();
    });

    

    this.selectedTab = 1;
  }

  applyRtuMonitoringModeToTraces() {
    for (const rtu of this.rtus) {
      for (const child of rtu.children) {
        if (child.childType === ChildType.Trace) {
          const trace = child as TraceDto;
          trace.rtuMonitoringMode = rtu.monitoringMode;
        }

        if (child.childType === ChildType.Otau) {
          const otau = child as OtauWebDto;
          for (const otauChild of otau.children) {
            if (otauChild.childType === ChildType.Trace) {
              const trace = otauChild as TraceDto;
              trace.rtuMonitoringMode = rtu.monitoringMode;
            }
          }
        }
      }
    }
  }
}
