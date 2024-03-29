import { Injectable } from "@angular/core";
import { Subject } from "rxjs";

@Injectable({
  providedIn: "root",
})
export class AlarmsService {
  // Observable string source
  private initialAlarmsCameSource = new Subject();
  private opticalEventConfirmedSource = new Subject<number>();
  private networkEventConfirmedSource = new Subject<number>();
  private bopEventConfirmedSource = new Subject<number>();

  // Observable string stream
  initialAlarmsCame$ = this.initialAlarmsCameSource.asObservable();
  opticalEventConfirmed$ = this.opticalEventConfirmedSource.asObservable();
  networkEventConfirmed$ = this.networkEventConfirmedSource.asObservable();
  bopEventConfirmed$ = this.bopEventConfirmedSource.asObservable();

  constructor() {}

  // Service message commands
  processInitialAlarms() {
    this.initialAlarmsCameSource.next();
  }

  confirmOpticalEvent(sorFileId: number) {
    this.opticalEventConfirmedSource.next(sorFileId);
  }
  confirmNetworkEvent(ordinal: number) {
    this.networkEventConfirmedSource.next(ordinal);
  }
  confirmBopEvent(ordinal: number) {
    this.bopEventConfirmedSource.next(ordinal);
  }
}
