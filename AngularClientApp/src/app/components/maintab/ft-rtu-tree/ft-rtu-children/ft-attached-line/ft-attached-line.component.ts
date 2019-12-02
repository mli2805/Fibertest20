import { Component, OnInit, Input, Output, ViewChild } from '@angular/core';
import { TraceDto } from 'src/app/models/dtos/traceDto';
import { TraceStatisticsDto } from 'src/app/models/dtos/traceStatisticsDto';
import { InteractionsService } from '../../../../../interactionServices/leavesToMaintab/interactions.service';
import { InteractionsParameter } from '../../../../../interactionServices/leavesToMaintab/interactionsParameter';
import { InteractionsCommandType } from '../../../../../interactionServices/leavesToMaintab/interactionsCommandType';
import { MatMenuTrigger, MatMenu } from '@angular/material';

@Component({
  selector: 'ft-attached-line',
  templateUrl: './ft-attached-line.component.html',
  styleUrls: ['./ft-attached-line.component.scss']
})
export class FtAttachedLineComponent implements OnInit {
  @Input() trace: TraceDto;

  @ViewChild(MatMenuTrigger, null)
  contextMenu: MatMenuTrigger;
  contextMenuPosition = { x: '0px', y: '0px' };

  constructor(private interactionsService: InteractionsService) {}

  ngOnInit() {}

  onContextMenu(event: MouseEvent) {
    event.preventDefault();
    this.contextMenuPosition.x = event.clientX + 'px';
    this.contextMenuPosition.y = event.clientY + 'px';
    this.contextMenu.menuData = { item: this.trace.title };
    this.contextMenu.openMenu();
    this.contextMenu.focus('mouse');
  }

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
