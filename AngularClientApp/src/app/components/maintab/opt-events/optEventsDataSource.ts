import { DataSource } from '@angular/cdk/table';
import { OptEventDto } from 'src/app/models/dtos/optEventDto';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { OptEvService } from 'src/app/api/oev-api.service';
import { CollectionViewer } from '@angular/cdk/collections';
import { catchError, finalize } from 'rxjs/operators';

export class OptEventsDataSource implements DataSource<OptEventDto> {
  private optEventsSubject = new BehaviorSubject<OptEventDto[]>([]);
  private loadingSubject = new BehaviorSubject<boolean>(false);

  public loading$ = this.loadingSubject.asObservable();

  constructor(private oevApiService: OptEvService) {}

  connect(collectionViewer: CollectionViewer): Observable<OptEventDto[]> {
    return this.optEventsSubject.asObservable();
  }

  disconnect(collectionViewer: CollectionViewer): void {
    this.optEventsSubject.complete();
    this.loadingSubject.complete();
  }

  loadOptEvents(
    filter = '',
    sortOrder = 'asc',
    pageNumber = 0,
    pageSize = 13
  ) {
    this.loadingSubject.next(true);

    this.oevApiService
      .getEvents(filter, sortOrder, pageNumber, pageSize)
      .pipe(
        catchError(() => of([])),
        finalize(() => this.loadingSubject.next(false))
      )
      .subscribe((res: OptEventDto[]) => {
        // console.log(res);
        this.optEventsSubject.next(res);
      });
  }
}
