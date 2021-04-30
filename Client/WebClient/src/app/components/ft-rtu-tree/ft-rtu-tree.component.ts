import {
  Component,
  OnInit,
  OnDestroy,
  HostListener,
  Inject,
  AfterViewChecked,
} from "@angular/core";
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
import { NetworkEventDto } from "src/app/models/dtos/networkEventDto";
import { ChannelEvent } from "src/app/models/enums/channelEvent";
import { RtuPartState } from "src/app/models/enums/rtuPartState";
import { BopEventDto } from "src/app/models/dtos/bopEventDto";
import { TraceStateDto } from "src/app/models/dtos/trace/traceStateDto";
import { DOCUMENT } from "@angular/common";
import { FiberState } from "src/app/models/enums/fiberState";
import { BaseRefType } from "src/app/models/enums/baseRefType";

@Component({
  selector: "ft-rtu-tree",
  templateUrl: "./ft-rtu-tree.component.html",
  styleUrls: ["./ft-rtu-tree.component.css"],
})
@HostListener("window:scroll", ["$event"]) // for window scroll events
export class FtRtuTreeComponent implements OnInit, OnDestroy, AfterViewChecked {
  private previousRtus: RtuDto[];
  rtus: RtuDto[];
  private scrollPosition: number;
  public isNotLoaded = true;
  public destroyed = new Subject<any>();

  private fetchDataSubscription: Subscription;
  private monitoringStoppedSubscription: Subscription;
  private monitoringStartedSubscription: Subscription;
  private measurementAddedSubscription: Subscription;
  private networkEventAddedSubscription: Subscription;
  private bopEventAddedSubscription: Subscription;

  constructor(
    private oneApiService: OneApiService,
    private signalRService: SignalrService,
    private router: Router,
    private refreshTreeRequestEventService: FtRtuTreeEventService,
    @Inject(DOCUMENT) private doc: Document
  ) {
    this.isNotLoaded = true;
  }

  ngAfterViewChecked() {
    if (this.scrollPosition !== 0) {
      const div = this.doc.getElementById("outletDivId");
      div.scrollTop = this.scrollPosition;
      this.scrollPosition = 0;
    }
  }

  ngOnInit() {
    console.log("ngOnInit of ft-rtu-tree.component");
    this.fetchData();
    this.saveExpandeds();
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
        if (evnt === RtuTreeEvent.hideSpinner) {
          this.isNotLoaded = false;
        }
        if (evnt === RtuTreeEvent.fetchTree) {
          this.fetchData();
        }
        if (evnt === RtuTreeEvent.saveExpanded) {
          this.saveExpandeds();
        }
      });

    this.fetchDataSubscription = this.signalRService.fetchTreeEmitter.subscribe(
      () => {
        console.log(`fetch signal received`);
        this.fetchData();
      }
    );

    this.monitoringStoppedSubscription = this.signalRService.monitoringStoppedEmitter.subscribe(
      (signal: any) => {
        this.fetchData();
      }
    );

    this.monitoringStartedSubscription = this.signalRService.monitoringStartedEmitter.subscribe(
      (signal: any) => {
        this.fetchData();
      }
    );

    this.measurementAddedSubscription = this.signalRService.measurementAddedEmitter.subscribe(
      (signal: TraceStateDto) => {
        const rtu = this.rtus.find((r) => r.rtuId === signal.rtuId);
        for (const child of rtu.children) {
          if (child.childType === ChildType.Trace) {
            const tr = child as TraceDto;
            if (tr.traceId === signal.traceId) {
              tr.state = this.buildState(signal);
              return;
            }
          } else if (child.childType === ChildType.Otau) {
            const otau = child as OtauWebDto;
            for (const grandchild of otau.children) {
              if (grandchild.childType === ChildType.Trace) {
                const tr = grandchild as TraceDto;
                if (tr.traceId === signal.traceId) {
                  tr.state = this.buildState(signal);
                  return;
                }
              }
            }
          }
        }

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

  private buildState(dto: TraceStateDto) {
    if (dto.traceState === FiberState.Ok) {
      return FiberState.Ok;
    } else if (dto.baseRefType === BaseRefType.Fast) {
      return FiberState.Suspicion;
    } else {
      return dto.traceState;
    }
  }

  ngOnDestroy() {
    this.fetchDataSubscription.unsubscribe();
    this.monitoringStoppedSubscription.unsubscribe();
    this.monitoringStartedSubscription.unsubscribe();
    this.measurementAddedSubscription.unsubscribe();
    this.networkEventAddedSubscription.unsubscribe();
    this.bopEventAddedSubscription.unsubscribe();
    this.destroyed.next();
    this.destroyed.complete();
  }

  async fetchData() {
    this.isNotLoaded = true;
    const res = (await this.oneApiService
      .getRequest(`rtu/tree`)
      .toPromise()) as RtuDto[];

    console.log("rtu tree received", res);
    this.rtus = res;
    this.applyStoredExpandeds();
    this.applyMonitoringMode();
    const pos = sessionStorage.getItem("scrollTop");
    this.scrollPosition = +pos;
    this.isNotLoaded = false;
  }

  back() {
    console.log("window.history.length", window.history.length);
  }

  saveExpandeds() {
    if (this.rtus === undefined) {
      return;
    }
    const expandeds = {};
    for (const rtu of this.rtus) {
      expandeds[rtu.rtuId] = rtu.expanded;
      for (const child of rtu.children) {
        if (child != null && child.childType === ChildType.Otau) {
          const otau = child as OtauWebDto;
          expandeds[otau.otauId] = otau.expanded;
        }
      }
    }
    sessionStorage.setItem("expandeds", JSON.stringify(expandeds));
  }

  applyStoredExpandeds() {
    const value = sessionStorage.getItem("expandeds");
    if (value === null) {
      return;
    }
    const expandeds = JSON.parse(value);
    for (const rtu of this.rtus) {
      const previous = expandeds[rtu.rtuId];
      rtu.expanded = previous !== undefined ? previous : false;
      for (const child of rtu.children) {
        if (child != null && child.childType === ChildType.Otau) {
          const otau = child as OtauWebDto;
          const previousOtauExtended = expandeds[otau.otauId];
          otau.expanded =
            previousOtauExtended !== undefined ? previousOtauExtended : false;
        }
      }
    }
  }

  applyMonitoringMode() {
    for (const rtu of this.rtus) {
      const mode = rtu.monitoringMode;
      for (const child of rtu.children) {
        if (child != null && child.childType === ChildType.Trace) {
          const trace = child as TraceDto;
          trace.rtuMonitoringMode = mode;
        } else if (child != null && child.childType === ChildType.Otau) {
          const otau = child as OtauWebDto;
          for (const grandChild of otau.children) {
            if (
              grandChild != null &&
              grandChild.childType === ChildType.Trace
            ) {
              const trace = grandChild as TraceDto;
              trace.rtuMonitoringMode = mode;
            }
          }
        }
      }
    }
  }
}
