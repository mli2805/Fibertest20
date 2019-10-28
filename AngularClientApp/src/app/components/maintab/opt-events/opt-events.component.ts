import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { OptEvService } from 'src/app/api/oev-api.service';
import { OptEventsDataSource } from '../opt-events/optEventsDataSource';
import { MatPaginator, MatSort } from '@angular/material';
import { tap } from 'rxjs/operators';

@Component({
  selector: 'ft-opt-events',
  templateUrl: './opt-events.component.html',
  styleUrls: ['./opt-events.component.scss']
})
export class FtOptEventsComponent implements OnInit, AfterViewInit {
  displayedColumns = [
    'eventId',
    'measurementTimestamp',
    'eventRegistrationTimestamp',
    'rtuTitle',
    'traceTitle',
    'traceState',
    'eventStatus',
    'statusChangedTimestamp',
    'statusChangedByUser'
  ];
  dataSource: OptEventsDataSource;
  optEventCount = 5000; // TODO how to know

  @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
  @ViewChild(MatSort, { static: true }) sort: MatSort;

  constructor(private oevApiService: OptEvService) {}

  ngOnInit() {
    this.dataSource = new OptEventsDataSource(this.oevApiService);
    this.dataSource.loadOptEvents('', 'desc', 0, 13);
  }

  ngAfterViewInit() {
    this.paginator.page.pipe(tap(() => this.loadEventsPage())).subscribe();
  }

  loadEventsPage() {
    this.dataSource.loadOptEvents(
      '',
      'desc',
      this.paginator.pageIndex,
      this.paginator.pageSize
    );
  }

  onRowClicked(row) {
    console.log('Row clicked: ', row);
  }
}
