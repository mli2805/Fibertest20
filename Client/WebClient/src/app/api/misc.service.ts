import { Injectable } from "@angular/core";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Utils } from "../Utils/utils";

@Injectable({
  providedIn: "root"
})
export class MiscService {
  constructor(private httpClient: HttpClient) {}

  getAbout() {
    const url = Utils.GetWebApiUrl() + "/misc/about/";
    const currentUser = JSON.parse(sessionStorage.currentUser);

    const myHeaders = new HttpHeaders({
      Authorization: "Bearer " + currentUser.jsonWebToken
    });

    return this.httpClient.get(url, { headers: myHeaders });
  }
}
