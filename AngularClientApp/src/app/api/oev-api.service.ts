import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class OptEvService {

  constructor(private httpClient: HttpClient) { }

  getAllEvents() {
    const url = 'http://' + window.location.hostname + ':11837/oev';
    return this.httpClient.get(url);
  }
}
