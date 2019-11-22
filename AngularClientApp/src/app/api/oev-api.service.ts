import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Utils } from '../Utils/utils';
import { globalVars } from '../global-vars';

@Injectable({
  providedIn: 'root'
})
export class OptEvService {
  constructor(private httpClient: HttpClient) {}

  getAllEvents() {
    const url = Utils.GetWebApiUrl() + '/oev/getAll';
    return this.httpClient.get(url);
  }

  getEvents(
    isCurrentEvents = 'true',
    filterRtu = '',
    filterTrace = '',
    sortOrder = 'desc',
    pageNumber = 0,
    pageSize = 100
  ) {
    const url = Utils.GetWebApiUrl() + '/oev/getPage';

    const myHttpOptions = {
      headers: {
        Authorization:
          'Bearer ' + globalVars.globalVarSet.loggedUser.jsonWebToken
      },
      params: {
        isCurrentEvents,
        filterRtu,
        filterTrace,
        sortOrder,
        pageNumber: pageNumber.toString(),
        pageSize: pageSize.toString()
      }
    };

    return this.httpClient.get(url, myHttpOptions);
  }
}
