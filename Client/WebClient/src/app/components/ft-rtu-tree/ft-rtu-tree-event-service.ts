import { BehaviorSubject } from "rxjs";

export class FtRtuTreeEventService {
  private grandChildEvent = new BehaviorSubject<boolean>(false);

  emitEvent(value: boolean) {
    this.grandChildEvent.next(value);
  }

  grandChildEventListener() {
    return this.grandChildEvent.asObservable();
  }
}
