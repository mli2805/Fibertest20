import { DataSource } from "@angular/cdk/table";
import {
  BopEventDto,
  BopEventRequestDto,
} from "src/app/models/dtos/bopEventDto";
import { BehaviorSubject, Observable, of } from "rxjs";
import { OneApiService } from "src/app/api/one.service";
import { CollectionViewer } from "@angular/cdk/collections";
import { catchError, finalize } from "rxjs/operators";

export class BopEventsDataSource implements DataSource<BopEventDto> {
  private BopEventsSubject = new BehaviorSubject<BopEventDto[]>([]);
  private fullCountSubject = new BehaviorSubject<number>(5);
  private loadingSubject = new BehaviorSubject<boolean>(false);

  public loading$ = this.loadingSubject.asObservable();
  public fullCount = this.fullCountSubject.asObservable();

  constructor(private oneApiService: OneApiService) {}

  connect(collectionViewer: CollectionViewer): Observable<BopEventDto[]> {
    return this.BopEventsSubject.asObservable();
  }

  disconnect(collectionViewer: CollectionViewer): void {
    this.BopEventsSubject.complete();
    this.loadingSubject.complete();
  }

  async loadBopEvents(
    isCurrentEvents = "true",
    filterRtu = "",
    sortOrder = "asc",
    pageNumber = 0,
    pageSize = 13
  ) {
    this.loadingSubject.next(true);

    const params = {
      isCurrentEvents,
      filterRtu,
      sortOrder,
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString(),
    };

    const res = (await this.oneApiService
      .getRequest("tables/getBopsPage", params)
      .pipe(
        catchError(() => of([])),
        finalize(() => this.loadingSubject.next(false))
      )
      .toPromise()) as BopEventRequestDto;
    console.log(
      `${res.eventPortion.length} of ${res.fullCount} Bop event(s) received`
    );
    console.log(res);
    this.BopEventsSubject.next(res.eventPortion);
    this.fullCountSubject.next(res.fullCount);
  }
}
