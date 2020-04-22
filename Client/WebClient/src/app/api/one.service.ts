import { Injectable } from "@angular/core";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Utils } from "../Utils/utils";

@Injectable({
  providedIn: "root",
})
export class OneApiService {
  constructor(private httpClient: HttpClient) {}

  getRequest(request: string) {
    const url = Utils.GetWebApiUrl() + `/${request}`;
    const currentUser = JSON.parse(sessionStorage.currentUser);

    const myHeaders = new HttpHeaders({
      Authorization: "Bearer " + currentUser.jsonWebToken,
    });

    return this.httpClient.get(url, { headers: myHeaders });
  }

  postRequest(controller: string, request: string, body: any) {
    const url = Utils.GetWebApiUrl() + `/${controller}/${request}`;
    const currentUser = JSON.parse(sessionStorage.currentUser);

    const myHeaders = new HttpHeaders({
      Authorization: "Bearer " + currentUser.jsonWebToken,
    });
    return this.httpClient.post(url, body, { headers: myHeaders });
  }
}
