import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class TraceApiService {
  constructor(private httpClient: HttpClient) {}

  getAllTraces() {
    return this.httpClient.get(environment.baseUrl + '/trace');
  }
}
