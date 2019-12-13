import { Injectable } from "@angular/core";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Utils } from "../Utils/utils";

@Injectable({
  providedIn: "root"
})
export class TraceApiService {
  constructor(private httpClient: HttpClient) {}

  getOneTrace(id: string, request: string) {
    const url = Utils.GetWebApiUrl() + `/trace/${request}/${id}`;
    const currentUser = JSON.parse(sessionStorage.currentUser);

    const myHeaders = new HttpHeaders({
      Authorization: "Bearer " + currentUser.jsonWebToken
    });

    return this.httpClient.get(url, { headers: myHeaders });
  }
}
