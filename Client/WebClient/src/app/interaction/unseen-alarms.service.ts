import { Injectable } from "@angular/core";
import { Subject } from "rxjs";

@Injectable({
  providedIn: "root",
})
export class UnseenAlarmsService {
  // Observable string source
  private opticalEventConfirmedSource = new Subject<number>();
  private networkEventConfirmedSource = new Subject<number>();
  // Observable string stream
  opticalEventConfirmed$ = this.opticalEventConfirmedSource.asObservable();
  networkEventConfirmed$ = this.networkEventConfirmedSource.asObservable();

  constructor() {}

  // Service message commands
  confirmOpticalEvent(sorFileId: number) {
    this.opticalEventConfirmedSource.next(sorFileId);
  }
  confirmNetworkEvent(ordinal: number) {
    this.networkEventConfirmedSource.next(ordinal);
  }
}
