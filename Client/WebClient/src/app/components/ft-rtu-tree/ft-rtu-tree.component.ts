import { Component, OnInit, OnDestroy } from "@angular/core";
import { RtuDto } from "src/app/models/dtos/rtuTree/rtuDto";
import { ChildType } from "src/app/models/enums/childType";
import { TraceDto } from "src/app/models/dtos/rtuTree/traceDto";
import { OtauWebDto } from "src/app/models/dtos/rtuTree/otauWebDto";
import { Router, RouterEvent, NavigationEnd } from "@angular/router";
import { filter, takeUntil } from "rxjs/operators";
import { Subject, Subscription } from "rxjs";
import {
  FtRtuTreeEventService,
  RtuTreeEvent,
} from "./ft-rtu-tree-event-service";
import { OneApiService } from "src/app/api/one.service";
import { SignalrService } from "src/app/api/signalr.service";
import { MonitoringMode } from "src/app/models/enums/monitoringMode";
import { NetworkEventDto } from "src/app/models/dtos/networkEventDto";
import { ChannelEvent } from "src/app/models/enums/channelEvent";
import { RtuPartState } from "src/app/models/enums/rtuPartState";
import { BopEventDto } from "src/app/models/dtos/bopEventDto";
import { TraceStateDto } from "src/app/models/dtos/trace/traceStateDto";

@Component({
  selector: "ft-rtu-tree",
  templateUrl: "./ft-rtu-tree.component.html",
  styleUrls: ["./ft-rtu-tree.component.css"],
})
export class FtRtuTreeComponent implements OnInit, OnDestroy {
  private previousRtus: RtuDto[];
  private rtus: RtuDto[];
  public isNotLoaded = true;
  public destroyed = new Subject<any>();

  private monitoringStoppedSubscription: Subscription;
  private monitoringStartedSubscription: Subscription;
  private measurementAddedSubscription: Subscription;
  private networkEventAddedSubscription: Subscription;
  private bopEventAddedSubscription: Subscription;

  constructor(
    private oneApiService: OneApiService,
    private signalRService: SignalrService,
    private router: Router,
    private refreshTreeRequestEventService: FtRtuTreeEventService
  ) {
    this.isNotLoaded = true;
  }

  ngOnInit() {
    this.fetchData();
    // https://medium.com/angular-in-depth/refresh-current-route-in-angular-512a19d58f6e
    this.router.events
      .pipe(
        filter((event: RouterEvent) => event instanceof NavigationEnd),
        takeUntil(this.destroyed)
      )
      .subscribe(() => {
        this.fetchData();
      });

    this.refreshTreeRequestEventService
      .refreshTreeRequestEventListener()
      .subscribe((evnt: RtuTreeEvent) => {
        console.log("Event: ", evnt);
        if (evnt === RtuTreeEvent.showSpinner) {
          this.isNotLoaded = true;
        }
        if (evnt === RtuTreeEvent.fetchTree) {
          this.fetchData();
        }
      });

    this.monitoringStoppedSubscription = this.signalRService.monitoringStoppedEmitter.subscribe(
      (signal: any) => {
        // const rtu = this.rtus.find((r) => r.rtuId === signal.rtuId);
        // rtu.monitoringMode = MonitoringMode.Off;
        this.fetchData();
      }
    );

    this.monitoringStartedSubscription = this.signalRService.monitoringStartedEmitter.subscribe(
      (signal: any) => {
        // const rtu = this.rtus.find((r) => r.rtuId === signal.rtuId);
        // rtu.monitoringMode = MonitoringMode.On;
        this.fetchData();
      }
    );

    this.measurementAddedSubscription = this.signalRService.measurementAddedEmitter.subscribe(
      (signal: TraceStateDto) => {
        const rtu = this.rtus.find((r) => r.rtuId === signal.rtuId);
        const trace = rtu.children.find(
          (t) =>
            t.childType === ChildType.Trace &&
            (t as TraceDto).traceId === signal.traceId
        ) as TraceDto;
        if (trace !== undefined) {
          trace.state = signal.traceState;
        }
      }
    );

    this.networkEventAddedSubscription = this.signalRService.networkEventAddedEmitter.subscribe(
      (signal: NetworkEventDto) => {
        const rtu = this.rtus.find((r) => r.rtuId === signal.rtuId);
        if (signal.onMainChannel === ChannelEvent.Repaired) {
          rtu.mainChannelState = RtuPartState.Ok;
        }
        if (signal.onMainChannel === ChannelEvent.Broken) {
          rtu.mainChannelState = RtuPartState.Broken;
        }
        if (signal.onReserveChannel === ChannelEvent.Repaired) {
          rtu.reserveChannelState = RtuPartState.Ok;
        }
        if (signal.onReserveChannel === ChannelEvent.Broken) {
          rtu.reserveChannelState = RtuPartState.Broken;
        }
      }
    );

    this.bopEventAddedSubscription = this.signalRService.bopEventAddedEmitter.subscribe(
      (signal: BopEventDto) => {
        const rtu = this.rtus.find((r) => r.rtuId === signal.rtuId);

        const otau = rtu.children.find(
          (o) =>
            o.childType === ChildType.Otau &&
            (o as OtauWebDto).serial === signal.serial
        ) as OtauWebDto;
        otau.isOk = signal.bopState;

        const badBop = rtu.children.find(
          (c) =>
            c.childType === ChildType.Otau && (c as OtauWebDto).isOk === false
        );
        if (badBop === undefined) {
          rtu.bopState = RtuPartState.Ok;
        } else {
          rtu.bopState = RtuPartState.Broken;
        }
      }
    );
  }

  ngOnDestroy() {
    this.monitoringStoppedSubscription.unsubscribe();
    this.monitoringStartedSubscription.unsubscribe();
    this.measurementAddedSubscription.unsubscribe();
    this.networkEventAddedSubscription.unsubscribe();
    this.bopEventAddedSubscription.unsubscribe();
    this.destroyed.next();
    this.destroyed.complete();
  }

  fetchData() {
    this.isNotLoaded = true;
    this.oneApiService.getRequest(`rtu/tree`).subscribe((res: RtuDto[]) => {
      console.log("rtu tree received", res);
      this.rtus = res;
      this.applyRtuMonitoringModeToTraces();
      this.isNotLoaded = false;
    });
  }

  back() {
    console.log("window.history.length", window.history.length);
  }

  applyRtuMonitoringModeToTraces() {
    for (const rtu of this.rtus) {
      rtu.expanded = this.getPreviousIsExpanded(rtu);
      for (const child of rtu.children) {
        if (child.childType === ChildType.Trace) {
          const trace = child as TraceDto;
          trace.rtuMonitoringMode = rtu.monitoringMode;
        }

        if (child.childType === ChildType.Otau) {
          const otau = child as OtauWebDto;
          otau.expanded = this.getOtauPreviousIsExpanded(rtu, otau);
          for (const otauChild of otau.children) {
            if (otauChild.childType === ChildType.Trace) {
              const trace = otauChild as TraceDto;
              trace.rtuMonitoringMode = rtu.monitoringMode;
            }
          }
        }
      }
    }
    this.previousRtus = this.rtus;
  }

  getPreviousIsExpanded(rtu: RtuDto): boolean {
    if (this.previousRtus === undefined) {
      return false;
    }
    const prev = this.previousRtus.find((prtu) => prtu.rtuId === rtu.rtuId);
    return prev === undefined ? false : prev.expanded;
  }

  getOtauPreviousIsExpanded(rtu: RtuDto, otau: OtauWebDto): boolean {
    if (this.previousRtus === undefined) {
      return false;
    }
    const prevRtu = this.previousRtus.find((prtu) => prtu.rtuId === rtu.rtuId);
    const prevOtau = prevRtu.children.find(
      (potau) => potau.port === otau.port
    ) as OtauWebDto;
    return prevOtau === undefined ? false : prevOtau.expanded;
  }
}
