import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})

export class RtuApiService {

  baseUrl = 'https://localhost:44304/api';

  constructor(private httpClient: HttpClient) {}

  getAllRtu() {
    return this.httpClient.get(this.baseUrl + '/rtu');
  }
}
