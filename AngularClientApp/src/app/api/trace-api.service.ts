import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Utils } from '../Utils/utils';

@Injectable({
  providedIn: 'root'
})

export class TraceApiService {
  constructor(private httpClient: HttpClient) {}

  getTraceInformation(id: string) {
    const url = Utils.GetWebApiUrl() + '/trace/information/' + id;

    return this.httpClient.get(url);
  }

  getTraceStatistics(id: string) {
    const url = Utils.GetWebApiUrl() + '/trace/statistics/' + id;

    return this.httpClient.get(url);
  }
}
