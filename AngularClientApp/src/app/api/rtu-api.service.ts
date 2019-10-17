import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class RtuApiService {
  constructor(private httpClient: HttpClient) {}

  getAllRtu() {
    const url = 'http://' + window.location.hostname + ':11837/api/rtu';
    return this.httpClient.get(url);
  }
}
