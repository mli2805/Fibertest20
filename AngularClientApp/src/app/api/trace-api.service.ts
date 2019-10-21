import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class TraceApiService {
  constructor(private httpClient: HttpClient) {}

  getTraceStatistics(id: string) {
    const url =
      'http://' + window.location.hostname + ':11837/trace/statistics/' + id;

    return this.httpClient.get(url);
  }
}
