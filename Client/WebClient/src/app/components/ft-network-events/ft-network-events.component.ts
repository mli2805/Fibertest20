// https://blog.angular-university.io/angular-material-data-table/

import {
  Component,
  OnInit,
  ViewChild,
  AfterViewInit,
  ElementRef,
} from "@angular/core";
import { MatPaginator, MatSort, MatMenuTrigger } from "@angular/material";
import { tap, debounceTime, distinctUntilChanged } from "rxjs/operators";
import { merge, fromEvent } from "rxjs";
import { OneApiService } from "src/app/api/one.service";
import { NetworkEventsDataSource } from "./networkEventsDataSource";
import { UnseenAlarmsService } from "src/app/interaction/unseen-alarms.service";
import { SignalrService } from "src/app/api/signalr.service";

@Component({
  selector: "ft-network-events",
  templateUrl: "./ft-network-events.component.html",
  styleUrls: ["./ft-network-events.component.css"],
})
export class FtNetworkEventsComponent implements OnInit, AfterViewInit {
  labelPosition = "before";
  sliderColor = "accent";
  isCurrentEvents: boolean;

  displayedColumns = [
    "eventId",
    "eventRegistrationTimestamp",
    "rtuTitle",
    "rtuAvailability",
    "mainChannelEvent",
    "reserveChannelEvent",
  ];
  dataSource: NetworkEventsDataSource;

  @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
  @ViewChild(MatSort, { static: true }) sort: MatSort;
  @ViewChild("inputRtu", { static: true }) inputRtu: ElementRef;

  @ViewChild(MatMenuTrigger, null)
  contextMenu: MatMenuTrigger;
  contextMenuPosition = { x: "0px", y: "0px" };

  constructor(
    private oneApiService: OneApiService,
    private signalRService: SignalrService,
    private unseenAlarmsService: UnseenAlarmsService
  ) {
    this.isCurrentEvents = true;
  }

  ngOnInit() {
    this.dataSource = new NetworkEventsDataSource(this.oneApiService);
    this.dataSource.loadNetworkEvents(
      String(this.isCurrentEvents),
      "",
      "desc",
      0,
      8
    );

    this.signalRService.networkEventAddedEmitter.subscribe(() =>
      this.loadPage()
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

    this.sort.sortChange.subscribe(() => (this.paginator.pageIndex = 0));

    merge(this.sort.sortChange, this.paginator.page)
      .pipe(tap(() => this.loadPage()))
      .subscribe();
  }

  loadPage() {
    this.dataSource.loadNetworkEvents(
      String(this.isCurrentEvents),
      this.inputRtu.nativeElement.value,
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

  seeEvent(row) {
    this.unseenAlarmsService.confirmNetworkEvent(row.eventId);
  }

  getRtuAvailability(isRtuAvailable: boolean): string {
    return isRtuAvailable ? "SID_Available" : "SID_Not_available";
  }
}
