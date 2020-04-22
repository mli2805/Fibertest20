import { Injectable } from "@angular/core";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Utils } from "../Utils/utils";

@Injectable({
  providedIn: "root",
})
export class OneApiService {
  constructor(private httpClient: HttpClient) {}

  getRequest(request: string, params = null) {
    const url = Utils.GetWebApiUrl() + `/${request}`;
    const currentUser = JSON.parse(sessionStorage.currentUser);

    const myHttpOptions = {
      headers: {
        Authorization: "Bearer " + currentUser.jsonWebToken,
      },
      params,
    };

    return this.httpClient.get(url, myHttpOptions);
  }

  postRequest(request: string, body: any) {
    const url = Utils.GetWebApiUrl() + `/${request}`;
    const currentUser = JSON.parse(sessionStorage.currentUser);

    const myHeaders = new HttpHeaders({
      Authorization: "Bearer " + currentUser.jsonWebToken,
    });
    return this.httpClient.post(url, body, { headers: myHeaders });
  }
}
