import { Injectable } from "@angular/core";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Utils } from "../Utils/utils";

@Injectable({
  providedIn: "root",
})
export class AuthService {
  constructor(private httpClient: HttpClient) {}

  login(user: string, pw: string) {
    const url = Utils.GetWebApiUrl() + "/authentication/login/";
    console.log(`login url: ${url}`);
    const body = { username: user, password: pw };

    // !!!!!!!!!!!  press Ctrl+F5   !!!!!!!!!!!!!!!!!!
    // or even restart w3wp processes for webApi and webClient

    // const myHeaders = new HttpHeaders({
    //   // "Access-Control-Allow-Origin": "*",
    //   "Access-Control-Allow-Origin": "http://localhost:4200",
    // });

    return this.httpClient.post(url, body);

    // return this.httpClient.post(url, body, {
    //   headers: myHeaders,
    // });
  }

  changeGuidWithSignalrConnectionId(
    jwt: string,
    oldGuid: string,
    connId: string
  ) {
    const url = Utils.GetWebApiUrl() + "/authentication/changeConnectionId/";
    const body = { oldGuid, connId };
    const myHeaders = new HttpHeaders({
      Authorization: "Bearer " + jwt,
    });

    return this.httpClient.post(url, body, {
      headers: myHeaders,
    });
  }

  logout() {
    const url = Utils.GetWebApiUrl() + "/authentication/logout/";
    const currentUser = JSON.parse(sessionStorage.currentUser);
    const body = { username: currentUser.username };

    const myHeaders = new HttpHeaders({
      Authorization: "Bearer " + currentUser.jsonWebToken,
    });

    return this.httpClient.post(url, body, {
      headers: myHeaders,
    });
  }
}
