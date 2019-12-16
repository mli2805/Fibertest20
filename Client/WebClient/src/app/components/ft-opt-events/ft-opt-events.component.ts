// https://blog.angular-university.io/angular-material-data-table/

import {
  Component,
  OnInit,
  ViewChild,
  AfterViewInit,
  ElementRef,
} from "@angular/core";
import { OptEvService } from "src/app/api/oev.service";
import { OptEventsDataSource } from "../ft-opt-events/optEventsDataSource";
import { MatPaginator, MatSort } from "@angular/material";
import { tap, debounceTime, distinctUntilChanged } from "rxjs/operators";
import { merge, fromEvent } from "rxjs";

@Component({
  selector: "ft-opt-events",
  templateUrl: "./ft-opt-events.component.html",
  styleUrls: ["./ft-opt-events.component.css"]
})
export class FtOptEventsComponent implements OnInit, AfterViewInit {
  labelPosition = "before";
  sliderColor = "accent";
  isCurrentEvents: boolean;

  displayedColumns = [
    "eventId",
    "measurementTimestamp",
    "eventRegistrationTimestamp",
    "rtuTitle",
    "traceTitle",
    "traceState",
    "eventStatus",
    "statusChangedTimestamp",
    "statusChangedByUser"
  ];
  dataSource: OptEventsDataSource;
  optEventCount = 5000; // TODO how to know

  @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
  @ViewChild(MatSort, { static: true }) sort: MatSort;
  @ViewChild("inputRtu", { static: true }) inputRtu: ElementRef;
  @ViewChild("inputTrace", { static: true }) inputTrace: ElementRef;

  constructor(private oevApiService: OptEvService) {
    this.isCurrentEvents = true;
  }

  ngOnInit() {
    this.dataSource = new OptEventsDataSource(this.oevApiService);
    this.dataSource.loadOptEvents(
      String(this.isCurrentEvents),
      "",
      "",
      "desc",
      0,
      8
    );
  }

  ngAfterViewInit() {
    // server-side search
    fromEvent(this.inputRtu.nativeElement, "keyup")
      .pipe(
        debounceTime(150),
        distinctUntilChanged(),
        tap(() => {
          this.paginator.pageIndex = 0;
          this.loadEventsPage();
        })
      )
      .subscribe();

    fromEvent(this.inputTrace.nativeElement, "keyup")
      .pipe(
        debounceTime(150),
        distinctUntilChanged(),
        tap(() => {
          this.paginator.pageIndex = 0;
          this.loadEventsPage();
        })
      )
      .subscribe();

    this.sort.sortChange.subscribe(() => (this.paginator.pageIndex = 0));

    merge(this.sort.sortChange, this.paginator.page)
      .pipe(tap(() => this.loadEventsPage()))
      .subscribe();
  }

  loadEventsPage() {
    this.dataSource.loadOptEvents(
      String(this.isCurrentEvents),
      this.inputRtu.nativeElement.value,
      this.inputTrace.nativeElement.value,
      this.sort.direction,
      this.paginator.pageIndex,
      this.paginator.pageSize
    );
  }

  changedSlider() {
    this.paginator.pageIndex = 0;
    console.log(`slider changed, isCurrentEvents ${this.isCurrentEvents}`);
    this.loadEventsPage();
  }

  onRowClicked(row) {
    console.log("Row clicked: ", row);
  }
}
