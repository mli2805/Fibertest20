import { DataSource } from "@angular/cdk/table";
import { BehaviorSubject, Observable, of } from "rxjs";
import { CollectionViewer } from "@angular/cdk/collections";
import { catchError, finalize } from "rxjs/operators";
import { OneApiService } from "src/app/api/one.service";
import {
  NetworkEventDto,
  NetworkEventRequestDto,
} from "src/app/models/dtos/networkEventDto";

export class NetworkEventsDataSource implements DataSource<NetworkEventDto> {
  private networkEventsSubject = new BehaviorSubject<NetworkEventDto[]>([]);
  private fullCountSubject = new BehaviorSubject<number>(5);
  private loadingSubject = new BehaviorSubject<boolean>(false);

  public loading$ = this.loadingSubject.asObservable();
  public fullCount = this.fullCountSubject.asObservable();

  constructor(private oneApiService: OneApiService) {}

  connect(collectionViewer: CollectionViewer): Observable<NetworkEventDto[]> {
    return this.networkEventsSubject.asObservable();
  }

  disconnect(collectionViewer: CollectionViewer): void {
    this.networkEventsSubject.complete();
    this.loadingSubject.complete();
  }

  loadNetworkEvents(
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

    this.oneApiService
      .getRequest("tables/getNetworksPage", params)
      .pipe(
        catchError(() => of([])),
        finalize(() => this.loadingSubject.next(false))
      )
      .subscribe((res: NetworkEventRequestDto) => {
        console.log(
          `${res.eventPortion.length} of ${res.fullCount} network event(s) received`
        );
        console.log(res);
        this.networkEventsSubject.next(res.eventPortion);
        this.fullCountSubject.next(res.fullCount);
      });
  }
}
