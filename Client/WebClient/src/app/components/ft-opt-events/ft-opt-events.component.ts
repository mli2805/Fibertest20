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
import { AlarmsService } from "src/app/interaction/alarms.service";
import { SignalrService } from "src/app/api/signalr.service";
import { UpdateMeasurementDto } from "src/app/models/dtos/trace/updateMeasurementDto";
import { SorFileManager } from "src/app/utils/sorFileManager";

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
    private signalRService: SignalrService,
    private alarmsService: AlarmsService
  ) {
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

    this.signalRService.measurementAddedEmitter.subscribe(() =>
      this.loadPage()
    );
    this.signalRService.measurementUpdatedEmitter.subscribe(
      (signal: UpdateMeasurementDto) => {
        const line = this.dataSource.optEventsSubject.value.find(
          (l) => l.eventId === signal.sorFileId
        );
        line.eventStatus = signal.eventStatus;
        line.statusChangedTimestamp = signal.statusChangedTimestamp;
        line.statusChangedByUser = signal.statusChangedByUser;
        console.log("measurement updated");
      }
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
    this.alarmsService.confirmOpticalEvent(row.eventId);

    this.contextMenuPosition.x = event.clientX + "px";
    this.contextMenuPosition.y = event.clientY + "px";
    this.contextMenu.menuData = { row };
    this.contextMenu.openMenu();
    this.contextMenu.focus("mouse");
    event.preventDefault();
  }

  showRef(isBaseIncluded: boolean) {
    SorFileManager.Show(
      this.router,
      true,
      this.contextMenu.menuData.row.eventId,
      "",
      isBaseIncluded
    );
  }

  saveRef(isBaseIncluded: boolean) {
    SorFileManager.Download(
      this.oneApiService,
      this.contextMenu.menuData.row.eventId,
      isBaseIncluded,
      this.contextMenu.menuData.row.traceTitle,
      this.contextMenu.menuData.row.eventRegistrationTimestamp
    );
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

  seeEvent(row) {
    this.alarmsService.confirmOpticalEvent(row.eventId);
  }
}
