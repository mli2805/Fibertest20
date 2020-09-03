import { BehaviorSubject } from "rxjs";

// I use it when after some user's action rtu-tree should be re-read
// those are: attach/detach trace or otau and change of rtu monitoring mode

export class FtRtuTreeEventService {
  private refreshTreeRequestEvent = new BehaviorSubject<RtuTreeEvent>(null);

  emitEvent(value: RtuTreeEvent) {
    this.refreshTreeRequestEvent.next(value);
  }

  refreshTreeRequestEventListener() {
    return this.refreshTreeRequestEvent.asObservable();
  }
}

export enum RtuTreeEvent {
  showSpinner,
  hideSpinner,
  fetchTree,
}
