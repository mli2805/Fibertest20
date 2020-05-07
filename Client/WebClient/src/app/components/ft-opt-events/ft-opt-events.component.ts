// https://blog.angular-university.io/angular-material-data-table/

import {
  Component,
  OnInit,
  ViewChild,
  AfterViewInit,
  ElementRef,
} from "@angular/core";
import { OptEventsDataSource } from "../ft-opt-events/optEventsDataSource";
import { MatPaginator, MatSort, MatMenuTrigger } from "@angular/material";
import { tap, debounceTime, distinctUntilChanged } from "rxjs/operators";
import { merge, fromEvent } from "rxjs";
import { OneApiService } from "src/app/api/one.service";
import { OptEventDto } from "src/app/models/dtos/optEventDto";

@Component({
  selector: "ft-opt-events",
  templateUrl: "./ft-opt-events.component.html",
  styleUrls: ["./ft-opt-events.component.css"],
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
    "statusChangedByUser",
  ];
  dataSource: OptEventsDataSource;

  @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
  @ViewChild(MatSort, { static: true }) sort: MatSort;
  @ViewChild("inputRtu", { static: true }) inputRtu: ElementRef;
  @ViewChild("inputTrace", { static: true }) inputTrace: ElementRef;

  @ViewChild(MatMenuTrigger, null)
  contextMenu: MatMenuTrigger;
  contextMenuPosition = { x: "0px", y: "0px" };

  constructor(private oneApiService: OneApiService) {
    this.isCurrentEvents = true;
  }

  ngOnInit() {
    this.dataSource = new OptEventsDataSource(this.oneApiService);
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
          this.loadPage();
        })
      )
      .subscribe();

    fromEvent(this.inputTrace.nativeElement, "keyup")
      .pipe(
        debounceTime(150),
        distinctUntilChanged(),
        tap(() => {
          this.paginator.pageIndex = 0;
          this.loadPage();
        })
      )
      .subscribe();

    this.sort.sortChange.subscribe(() => (this.paginator.pageIndex = 0));

    merge(this.sort.sortChange, this.paginator.page)
      .pipe(tap(() => this.loadPage()))
      .subscribe();
  }

  loadPage() {
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
    this.loadPage();
  }

  onRowClicked(event: MouseEvent, row: OptEventDto) {
    event.preventDefault();
    this.contextMenuPosition.x = event.clientX + "px";
    this.contextMenuPosition.y = event.clientY + "px";
    this.contextMenu.menuData = { row };
    this.contextMenu.openMenu();
    this.contextMenu.focus("mouse");
  }

  onContextMenu(event: MouseEvent, row: OptEventDto) {
    event.preventDefault();
    this.contextMenuPosition.x = event.clientX + "px";
    this.contextMenuPosition.y = event.clientY + "px";
    this.contextMenu.menuData = { row };
    this.contextMenu.openMenu();
    this.contextMenu.focus("mouse");
  }

  onShowRef() {
    const oeDto: OptEventDto = Object.assign(
      new OptEventDto(),
      this.contextMenu.menuData.row
    );
    alert(`Click Show Ref for ${oeDto.eventId}`);
  }

  onShowTraceState() {
    const oeDto: OptEventDto = Object.assign(
      new OptEventDto(),
      this.contextMenu.menuData.row
    );
    alert(`Click Show Trace State for ${oeDto.eventId}`);
  }
}
