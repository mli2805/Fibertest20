import { Injectable } from "@angular/core";
import { Subject } from "rxjs";

@Injectable({
  providedIn: "root",
})
export class AlarmsService {
  // Observable string source
  private initialAlarmsCameSource = new Subject<string>();
  private opticalEventConfirmedSource = new Subject<number>();
  private networkEventConfirmedSource = new Subject<number>();

  // Observable string stream
  initialAlarmsCame$ = this.initialAlarmsCameSource.asObservable();
  opticalEventConfirmed$ = this.opticalEventConfirmedSource.asObservable();
  networkEventConfirmed$ = this.networkEventConfirmedSource.asObservable();

  constructor() {}

  // Service message commands
  processInitialAlarms(json: string) {
    this.initialAlarmsCameSource.next(json);
  }

  confirmOpticalEvent(sorFileId: number) {
    this.opticalEventConfirmedSource.next(sorFileId);
  }
  confirmNetworkEvent(ordinal: number) {
    this.networkEventConfirmedSource.next(ordinal);
  }
}
