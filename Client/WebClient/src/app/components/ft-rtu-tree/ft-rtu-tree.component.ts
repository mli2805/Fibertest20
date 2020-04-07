import { Component, OnInit, OnDestroy } from "@angular/core";
import { RtuApiService } from "src/app/api/rtu.service";
import { RtuDto } from "src/app/models/dtos/rtuTree/rtuDto";
import { ChildType } from "src/app/models/enums/childType";
import { TraceDto } from "src/app/models/dtos/rtuTree/traceDto";
import { OtauWebDto } from "src/app/models/dtos/rtuTree/otauWebDto";
import { Router, RouterEvent, NavigationEnd } from "@angular/router";
import { filter, takeUntil } from "rxjs/operators";
import { Subject } from "rxjs";

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

  constructor(private rtuService: RtuApiService, private router: Router) {
    this.isNotLoaded = true;
  }

  ngOnInit() {
    console.log("we are in ngOnInit");
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
  }

  ngOnDestroy() {
    this.destroyed.next();
    this.destroyed.complete();
  }

  fetchData() {
    this.isNotLoaded = true;
    this.rtuService.getAllRtu().subscribe((res: RtuDto[]) => {
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
}
