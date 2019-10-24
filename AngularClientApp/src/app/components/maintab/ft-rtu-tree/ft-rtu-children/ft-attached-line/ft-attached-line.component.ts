import { Component, OnInit, Input, Output } from '@angular/core';
import { TraceDto } from 'src/app/models/dtos/traceDto';
import { TraceApiService } from 'src/app/api/trace-api.service';
import { TraceStatisticsDto } from 'src/app/models/dtos/traceStatisticsDto';
import { InteractionsService } from '../../../interactions.service';
import { InteractionsParameter } from '../../../interactionsParameter';
import { InteractionsCommandType } from '../../../interactionsCommandType';

@Component({
  selector: 'ft-attached-line',
  templateUrl: './ft-attached-line.component.html',
  styleUrls: ['./ft-attached-line.component.scss']
})
export class FtAttachedLineComponent implements OnInit {
  @Input() trace: TraceDto;

  private traceStatistics: TraceStatisticsDto;
  constructor(
    private traceService: TraceApiService,
    private interactionsService: InteractionsService
  ) {}

  ngOnInit() {}

  displayInformation() {
    this.sendCommand(InteractionsCommandType.TraceInformation);
  }
  displayStatistics() {
    this.sendCommand(InteractionsCommandType.TraceStatistics);
  }

  sendCommand(commandType: InteractionsCommandType) {
    const cmd = new InteractionsParameter();
    cmd.commandType = commandType;
    cmd.traceId = this.trace.traceId;
    this.interactionsService.sendCommand(cmd);
  }
}