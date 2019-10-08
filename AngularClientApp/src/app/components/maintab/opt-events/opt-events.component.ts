import { Component, OnInit, ViewChild } from '@angular/core';
import { OptEvService } from 'src/app/api/oev-api.service';
import { OptEventDto } from 'src/app/models/optEventDto';
import { MatPaginator, MatTableDataSource, MatSort } from '@angular/material';
import { Utils } from 'src/app/Utils/utils';

@Component({
  selector: 'ft-opt-events',
  templateUrl: './opt-events.component.html',
  styleUrls: ['./opt-events.component.scss']
})
export class OptEventsComponent implements OnInit {
  // private optEventsArray: OptEventDto[];
  columnsToDisplay = [
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

  public datasource = new MatTableDataSource<OptEventDto>();

  @ViewChild(MatSort, { static: true }) sort: MatSort;

  constructor(private oevApiService: OptEvService) {}

  ngOnInit() {
    this.oevApiService.getAllEvents().subscribe((res: OptEventDto[]) => {
      this.datasource.data = res.sort(Utils.CompareOptEventDtos);
    });

    this.datasource.sort = this.sort;
  }
}
