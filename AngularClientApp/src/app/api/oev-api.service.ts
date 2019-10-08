import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class OptEvService {

  constructor(private httpClient: HttpClient) { }

  getAllEvents() {
    return this.httpClient.get(environment.baseUrl + '/oev');
  }
}
