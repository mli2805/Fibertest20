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
import { Router } from "@angular/router";
import { UnseenOpticalService } from "src/app/interaction/unseen-optical.service";

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

  constructor(
    private router: Router,
    private oneApiService: OneApiService,
    private unseenOpticalService: UnseenOpticalService
  ) {
    this.isCurrentEvents = false;
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

  onContextMenu(event: MouseEvent, row: OptEventDto) {
    this.contextMenuPosition.x = event.clientX + "px";
    this.contextMenuPosition.y = event.clientY + "px";
    this.contextMenu.menuData = { row };
    this.contextMenu.openMenu();
    this.contextMenu.focus("mouse");
    event.preventDefault();
  }

  showRef(param: number) {
    if (param === 1) {
      console.log("show ref: ", this.contextMenu.menuData.row.eventId);
    } else {
      console.log("show ref and base: ", this.contextMenu.menuData.row.eventId);
    }
  }

  saveRef(param: number) {
    if (param === 1) {
      console.log("save ref: ", this.contextMenu.menuData.row.eventId);
    } else {
      console.log("save ref and base: ", this.contextMenu.menuData.row.eventId);
    }
  }

  showRftsEvents() {
    console.log("show rfts events: ", this.contextMenu.menuData.row.eventId);
  }
  showTraceState() {
    const dict = {
      type: "fileId",
      traceId: null,
      fileId: this.contextMenu.menuData.row.eventId,
    };
    sessionStorage.setItem("traceStateParams", JSON.stringify(dict));
    this.router.navigate(["/trace-state"]);
  }

  seeEvent() {
    this.unseenOpticalService.confirmOpticalEvent(
      this.contextMenu.menuData.row.eventId
    );
  }
}
