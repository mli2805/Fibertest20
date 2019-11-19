import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Utils } from '../Utils/utils';
import { globalVars } from '../global-vars';

@Injectable({
  providedIn: 'root'
})

export class TraceApiService {
  constructor(private httpClient: HttpClient) {}

  getTraceInformation(id: string) {
    const url = Utils.GetWebApiUrl() + '/trace/information/' + id;
    const myHeaders = new HttpHeaders({
      Authorization : 'Bearer ' + globalVars.globalVarSet.loggedUser.jsonWebToken,
    });
    return this.httpClient.get(url, {headers: myHeaders});
  }

  getTraceStatistics(id: string) {
    const url = Utils.GetWebApiUrl() + '/trace/statistics/' + id;
    const myHeaders = new HttpHeaders({
      Authorization : 'Bearer ' + globalVars.globalVarSet.loggedUser.jsonWebToken,
    });
    return this.httpClient.get(url, {headers: myHeaders});
  }
}
