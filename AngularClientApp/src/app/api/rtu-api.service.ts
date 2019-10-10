import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class RtuApiService {
  constructor(private httpClient: HttpClient) {}

  getAllRtu() {
    return this.httpClient.get(environment.baseUrl + '/rtu');
  }
}
