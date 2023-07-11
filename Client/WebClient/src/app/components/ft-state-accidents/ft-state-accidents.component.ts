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
    private ts: TranslateService
  ) { 
    console.log("RTU state accidents c-tor hit!");
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
    this.alarmsService.confirmOpticalEvent(row.id);

    this.contextMenuPosition.x = event.clientX + "px";
    this.contextMenuPosition.y = event.clientY + "px";
    this.contextMenu.menuData = { row };
    this.contextMenu.openMenu();
    this.contextMenu.focus("mouse");
    event.preventDefault();
  }

  getState(dto: StateAccidentDto){
    return "state";
  }

  getExplanation(dto: StateAccidentDto){
    return "explanation"
  }

  getBackground(dto : StateAccidentDto){
    return "red";
  }

}
