import { Injectable } from "@angular/core";
import { Subject } from "rxjs";

@Injectable({
  providedIn: "root",
})
export class UnseenOpticalService {
  // Observable string source
  private opticalEventConfirmedSource = new Subject<number>();
  // Observable string stream
  opticalEventConfirmed$ = this.opticalEventConfirmedSource.asObservable();

  constructor() {}

  // Service message commands
  confirmOpticalEvent(sorFileId: number) {
    this.opticalEventConfirmedSource.next(sorFileId);
  }
}
