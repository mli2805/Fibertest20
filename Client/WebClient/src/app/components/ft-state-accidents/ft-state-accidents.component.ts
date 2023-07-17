import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';

import { MatMenuTrigger, MatPaginator, MatSort } from '@angular/material';

import { TranslateService } from '@ngx-translate/core';
import { OneApiService } from 'src/app/api/one.service';
import { SignalrService } from 'src/app/api/signalr.service';
import { AlarmsService } from 'src/app/interaction/alarms.service';
import { StateAccidentsDataSource } from './stateAccidentsDataSource';
import { fromEvent, merge } from 'rxjs';
import { debounceTime, distinctUntilChanged, tap } from 'rxjs/operators';
import { StateAccidentDto } from 'src/app/models/dtos/stateAccidentDto';
import { ReturnCode } from 'src/app/models/enums/returnCode';
import { ReturnCodePipe } from 'src/app/pipes/return-code.pipe';
import { BaseRefTypeFemalePipe, BaseRefTypeGenitivePipe } from 'src/app/pipes/base-ref-type.pipe'

@Component({
  selector: 'app-ft-state-accidents',
  templateUrl: './ft-state-accidents.component.html',
  styleUrls: ['./ft-state-accidents.component.css']
})
export class FtStateAccidentsComponent implements OnInit, AfterViewInit {
  labelPosition = "before";
  sliderColor = "accent";
  isCurrentEvents: boolean;

  displayedColumns = [
    "id",
    "eventRegistrationTimestamp",
    "rtuTitle",
    "traceTitle",
    "state",
    "explanation"
  ];
  dataSource: StateAccidentsDataSource;

  @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
  @ViewChild(MatSort, { static: true }) sort: MatSort;
  contextMenu: MatMenuTrigger;
  contextMenuPosition = { x: "0px", y: "0px" };

  constructor(
    private oneApiService: OneApiService,
    private signalRService: SignalrService,
    private alarmsService: AlarmsService,
    private ts: TranslateService,
    private returnCodePipe: ReturnCodePipe,
    private baseRefTypeFemalePipe: BaseRefTypeFemalePipe,
    private baseRefTypeGenitivePipe: BaseRefTypeGenitivePipe,
  ) { 
    this.isCurrentEvents = true;
  }

  ngOnInit() {
    this.dataSource = new StateAccidentsDataSource(this.oneApiService);
    this.dataSource.loadStateAccidents(
      String(this.isCurrentEvents),
      "desc",
      0,
      13
    );

    this.signalRService.stateAccidentEmitter.subscribe(()=>this.loadPage());
  }


  ngAfterViewInit() {
    // server-side search
    this.sort.sortChange.subscribe(() => (this.paginator.pageIndex = 0));

    merge(this.sort.sortChange, this.paginator.page)
      .pipe(tap(() => this.loadPage()))
      .subscribe();
  }

  loadPage() {
    this.dataSource.loadStateAccidents(
      String(this.isCurrentEvents),
      this.sort.direction,
      this.paginator.pageIndex,
      this.paginator.pageSize
    );
  }

  changedSlider() {
    this.paginator.pageIndex = 0;
    console.log(`slider changed, isCurrentEvents ${this.isCurrentEvents}`);
    this.loadPage();
  }
  
  onContextMenu(event: MouseEvent, row: StateAccidentDto) {
    this.alarmsService.confirmRtuStateAccident(row.id);

    event.preventDefault();
  }

  getState(dto: StateAccidentDto){
    if (dto.isMeasurementProblem){
      return dto.returnCode === ReturnCode.MeasurementEndedNormally 
        ? this.ts.instant("SID_Measurement__OK") : this.ts.instant("SID_Measurement__Failed_");
    }
    else {
      return dto.returnCode === ReturnCode.RtuManagerServiceWorking 
        ? this.ts.instant("SID_RTU__OK") : this.ts.instant("SID_RTU__Attention_required_");
    }
  }

  getExplanation(dto: StateAccidentDto){
    let returnCodeString = this.returnCodePipe.transform(dto.returnCode);

    if (dto.isMeasurementProblem){
      if (dto.returnCode === ReturnCode.MeasurementEndedNormally)
        return returnCodeString;
      else if (dto.returnCode === ReturnCode.MeasurementBaseRefNotFound)
        return `${this.baseRefTypeFemalePipe.transform(dto.baseRefType)} ${returnCodeString}`
      else {
        const genitive = this.baseRefTypeGenitivePipe.transform(dto.baseRefType);
        return returnCodeString.replace('{{0}}', genitive);
      }
    }
    else {
      return returnCodeString;
    }
  }

  getBackground(dto : StateAccidentDto){
    if (dto.returnCode === ReturnCode.MeasurementEndedNormally || dto.returnCode === ReturnCode.RtuManagerServiceWorking)
      return "transparent"
    return "red";
  }

}
