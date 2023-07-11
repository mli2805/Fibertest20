import { DataSource } from "@angular/cdk/collections";
import { StateAccidentDto, StateAccidentRequestDto } from "src/app/models/dtos/stateAccidentDto";

import { BehaviorSubject, Observable, of } from "rxjs";
import { CollectionViewer } from "@angular/cdk/collections";
import { catchError, finalize } from "rxjs/operators";
import { OneApiService } from "src/app/api/one.service";

export class StateAccidentsDataSource implements DataSource<StateAccidentDto> {
    public stateAccidentsSubject = new BehaviorSubject<StateAccidentDto[]>([]);
    private fullCountSubject = new BehaviorSubject<number>(5);
    private loadingSubject = new BehaviorSubject<boolean>(false);
  
    public loading$ = this.loadingSubject.asObservable();
    public fullCount = this.fullCountSubject.asObservable();

    constructor(private oneApiService: OneApiService) {}


    connect(collectionViewer: CollectionViewer): Observable<StateAccidentDto[]> {
        return this.stateAccidentsSubject.asObservable();
      }
    
      disconnect(collectionViewer: CollectionViewer): void {
        this.stateAccidentsSubject.complete();
        this.loadingSubject.complete();
        this.fullCountSubject.complete();
      }

    async loadStateAccidents(
        isCurrentEvents = "true",
        sortOrder = "asc",
        pageNumber = 0,
        pageSize = 13
    ){
        this.loadingSubject.next(true);

        const params = {
          isCurrentEvents,
          sortOrder,
          pageNumber: pageNumber.toString(),
          pageSize: pageSize.toString(),
        };

        const res = (await this.oneApiService
            .getRequest("tables/getStateAccidentPage", params)
            .pipe(
              catchError(() => of([])),
              finalize(() => this.loadingSubject.next(false))
            )
            .toPromise()) as StateAccidentRequestDto;
          console.log(
            `${res.accidentPortion.length} of ${res.fullCount} rtu accidents received`
          );
          this.stateAccidentsSubject.next(res.accidentPortion);
          this.fullCountSubject.next(res.fullCount);
    }
}