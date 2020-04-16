import { Injectable } from "@angular/core";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Utils } from "../Utils/utils";

@Injectable({
  providedIn: "root",
})
export class TraceApiService {
  constructor(private httpClient: HttpClient) {}

  getRequest(
    request: string,
    id: string,
    pageNumber: number = 0,
    pageSize: number = 13
  ) {
    const url = Utils.GetWebApiUrl() + `/trace/${request}/${id}`;
    const currentUser = JSON.parse(sessionStorage.currentUser);

    const myHttpOptions = {
      headers: {
        Authorization: "Bearer " + currentUser.jsonWebToken,
      },
      params: {
        pageNumber: pageNumber.toString(),
        pageSize: pageSize.toString(),
      },
    };

    return this.httpClient.get(url, myHttpOptions);
  }
}
