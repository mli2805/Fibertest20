import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Utils } from '../Utils/utils';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class RtuApiService {
  constructor(private httpClient: HttpClient) {}

  getAllRtu() {
    const url = Utils.GetWebApiUrl() + '/rtu';

    const myHeaders = new HttpHeaders({
      Authorization : 'Bearer ' + environment.jsonWebToken,
      MyHeader : 'Value of my header',
    });
    return this.httpClient.get(url, {headers: myHeaders});
  }
}
