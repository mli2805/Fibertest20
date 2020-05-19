import { DataSource } from "@angular/cdk/table";
import {
  OptEventDto,
  OptEventRequestDto,
} from "src/app/models/dtos/optEventDto";
import { BehaviorSubject, Observable, of } from "rxjs";
import { CollectionViewer } from "@angular/cdk/collections";
import { catchError, finalize } from "rxjs/operators";
import { OneApiService } from "src/app/api/one.service";

export class OptEventsDataSource implements DataSource<OptEventDto> {
  private optEventsSubject = new BehaviorSubject<OptEventDto[]>([]);
  private fullCountSubject = new BehaviorSubject<number>(5);
  private loadingSubject = new BehaviorSubject<boolean>(false);

  public loading$ = this.loadingSubject.asObservable();
  public fullCount = this.fullCountSubject.asObservable();

  constructor(private oneApiService: OneApiService) {}

  connect(collectionViewer: CollectionViewer): Observable<OptEventDto[]> {
    return this.optEventsSubject.asObservable();
  }

  disconnect(collectionViewer: CollectionViewer): void {
    this.optEventsSubject.complete();
    this.loadingSubject.complete();
    this.fullCountSubject.complete();
  }

  loadOptEvents(
    isCurrentEvents = "true",
    filterRtu = "",
    filterTrace = "",
    sortOrder = "asc",
    pageNumber = 0,
    pageSize = 13
  ) {
    this.loadingSubject.next(true);

    const params = {
      isCurrentEvents,
      filterRtu,
      filterTrace,
      sortOrder,
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString(),
    };

    this.oneApiService
      .getRequest("tables/getOpticalsPage", params)
      .pipe(
        catchError(() => of([])),
        finalize(() => this.loadingSubject.next(false))
      )
      .subscribe((res: OptEventRequestDto) => {
        console.log(
          `${res.eventPortion.length} of ${res.fullCount} optical event(s) received`
        );
        this.optEventsSubject.next(res.eventPortion);
        this.fullCountSubject.next(res.fullCount);
      });
  }
}
