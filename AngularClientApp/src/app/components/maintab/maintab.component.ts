import { Component, OnInit } from '@angular/core';
import { RtuApiService } from 'src/app/api/rtu-api.service';
import { RtuDto } from 'src/app/models/dtos/rtuDto';
import { ChildType } from 'src/app/models/enums/childType';
import { TraceDto } from 'src/app/models/dtos/traceDto';
import { OtauWebDto } from 'src/app/models/dtos/otauWebDto';
import { InteractionsService } from './interactions.service';
import { InteractionsCommandType } from './interactionsCommandType';
import { InteractionsParameter } from './interactionsParameter';
import { TraceApiService } from 'src/app/api/trace-api.service';
import { TraceStatisticsDto } from 'src/app/models/dtos/traceStatisticsDto';
import { DetailsViewModel } from './ft-tree-details/detailsViewModel';

@Component({
  selector: 'ft-maintab',
  templateUrl: './maintab.component.html',
  styleUrls: ['./maintab.component.scss'],
  providers: [InteractionsService]
})
export class FtMainTabComponent implements OnInit {
  private rtus: RtuDto[];
  private selectedTab: number;
  private detailsVm: DetailsViewModel;

  constructor(
    private rtuService: RtuApiService,
    private traceService: TraceApiService,
    private interactionsService: InteractionsService
  ) {
    interactionsService.commandRecieved$.subscribe(command => {
      this.reactCommand(command);
    });
  }

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

  reactCommand(parameter: InteractionsParameter) {
    switch (parameter.commandType) {
      case InteractionsCommandType.RtuState: {
        this.detailsVm = new DetailsViewModel(parameter.commandType);
        break;
      }
      case InteractionsCommandType.TraceInformation: {
        this.detailsVm = new DetailsViewModel(parameter.commandType);
        break;
      }
      case InteractionsCommandType.TraceStatistics: {
        this.traceService
          .getTraceStatistics(parameter.traceId)
          .subscribe((res: TraceStatisticsDto) => {
            this.detailsVm = new DetailsViewModel(parameter.commandType);
            this.detailsVm.data = res;
            console.log(res);
          });
        break;
      }
    }
  }
}
