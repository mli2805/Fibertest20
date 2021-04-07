import { Injectable } from "@angular/core";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Utils } from "../Utils/utils";

@Injectable({
  providedIn: "root",
})
export class AuthService {
  constructor(private httpClient: HttpClient) {}

  login(user: string, pw: string, version: string) {
    const url = Utils.GetWebApiUrl() + "/authentication/login/";
    console.log(`login url: ${url}`);
    const body = { username: user, password: pw, version };
    return this.httpClient.post(url, body);
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
    const body = { username: currentUser.username, connectionId: currentUser.connectionId };

    const myHeaders = new HttpHeaders({
      Authorization: "Bearer " + currentUser.jsonWebToken,
    });

    return this.httpClient.post(url, body, {
      headers: myHeaders,
    });
  }
}
