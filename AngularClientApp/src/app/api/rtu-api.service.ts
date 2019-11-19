import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Utils } from '../Utils/utils';
import { globalVars } from '../global-vars';

@Injectable({
  providedIn: 'root'
})
export class RtuApiService {
  constructor(private httpClient: HttpClient) {}

  getAllRtu() {
    const url = Utils.GetWebApiUrl() + '/rtu';

    const myHeaders = new HttpHeaders({
      Authorization : 'Bearer ' + globalVars.globalVarSet.loggedUser.jsonWebToken,
    });
    return this.httpClient.get(url, {headers: myHeaders});
  }
}
