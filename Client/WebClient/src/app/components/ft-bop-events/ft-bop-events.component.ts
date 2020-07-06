import {
  Component,
  OnInit,
  ViewChild,
  ElementRef,
  AfterViewInit,
} from "@angular/core";
import { BopEventsDataSource } from "./bopEventDataSource";
import { MatPaginator, MatSort, MatMenuTrigger } from "@angular/material";
import { OneApiService } from "src/app/api/one.service";
import { SignalrService } from "src/app/api/signalr.service";
import { AlarmsService } from "src/app/interaction/alarms.service";
import { fromEvent, merge } from "rxjs";
import { debounceTime, distinctUntilChanged, tap } from "rxjs/operators";
import { BopEventDto } from "src/app/models/dtos/bopEventDto";

@Component({
  selector: "ft-bop-events",
  templateUrl: "./ft-bop-events.component.html",
  styleUrls: ["./ft-bop-events.component.css"],
})
export class FtBopEventsComponent implements OnInit, AfterViewInit {
  labelPosition = "before";
  sliderColor = "accent";
  isCurrentEvents: boolean;

  displayedColumns = [
    "eventId",
    "eventRegistrationTimestamp",
    "bopAddress", // IP:port
    "rtuTitle",
    "bopState",
  ];
  dataSource: BopEventsDataSource;

  @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
  @ViewChild(MatSort, { static: true }) sort: MatSort;
  @ViewChild("inputRtu", { static: true }) inputRtu: ElementRef;

  @ViewChild(MatMenuTrigger, null)
  contextMenu: MatMenuTrigger;
  contextMenuPosition = { x: "0px", y: "0px" };

  constructor(
    private oneApiService: OneApiService,
    private signalRService: SignalrService,
    private alarmsService: AlarmsService
  ) {
    this.isCurrentEvents = true;
  }

  ngOnInit() {
    this.dataSource = new BopEventsDataSource(this.oneApiService);
    this.dataSource.loadBopEvents(
      String(this.isCurrentEvents),
      "",
      "desc",
      0,
      8
    );

    this.signalRService.bopEventAddedEmitter.subscribe(() => this.loadPage());
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
    this.dataSource.loadBopEvents(
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

  onContextMenu(event: MouseEvent, row: BopEventDto) {
    this.alarmsService.confirmBopEvent(row.eventId);

    event.preventDefault();
  }

  seeEvent(row) {
    this.alarmsService.confirmBopEvent(row.eventId);
  }

  getBopState(bopState: boolean): string {
    return bopState ? "SID_Ok" : "SID_Breakdown";
  }
}
