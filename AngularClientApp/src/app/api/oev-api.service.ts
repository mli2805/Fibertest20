import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Utils } from '../Utils/utils';

@Injectable({
  providedIn: 'root'
})
export class OptEvService {
  constructor(private httpClient: HttpClient) {}

  getAllEvents() {
    const url = Utils.GetWebApiUrl() + '/oev/getAll';
    return this.httpClient.get(url);
  }

  getEvents(
    filterRtu = '',
    filterTrace = '',
    sortOrder = 'desc',
    pageNumber = 0,
    pageSize = 100
  ) {
    const url = Utils.GetWebApiUrl() + '/oev/getPage';

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
