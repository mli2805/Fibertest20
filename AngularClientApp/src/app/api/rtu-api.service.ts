import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class RtuApiService {
  constructor(private httpClient: HttpClient) {}

  getAllRtu() {
    let url = window.location.hostname;
    url = url + ':11837/api';
    return this.httpClient.get(url + '/rtu');
  }
}
