import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class OptEvService {
  constructor(private httpClient: HttpClient) {}

  getAllEvents() {
    const url = 'http://' + window.location.hostname + ':11837/oev/getAll';
    return this.httpClient.get(url);
  }

  getEvents(
    filterRtu = '',
    filterTrace = '',
    sortOrder = 'desc',
    pageNumber = 0,
    pageSize = 100
  ) {
    const url = 'http://' + window.location.hostname + ':11837/oev/getPage';

    return this.httpClient.get(url, {
      params: new HttpParams()
        .set('filterRtu', filterRtu)
        .set('filterTrace', filterTrace)
        .set('sortOrder', sortOrder)
        .set('pageNumber', pageNumber.toString())
        .set('pageSize', pageSize.toString())
    });
  }
}
