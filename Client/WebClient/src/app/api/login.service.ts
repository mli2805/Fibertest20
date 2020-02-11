import { Injectable } from "@angular/core";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Utils } from "../Utils/utils";

@Injectable({
  providedIn: "root"
})
export class LoginService {
  constructor(private httpClient: HttpClient) {}

  login(user: string, pw: string) {
    const url = Utils.GetWebApiUrl() + "/authentication/login/";
    const body = { username: user, password: pw };

    return this.httpClient.post(url, body);
  }

  logout() {
    const url = Utils.GetWebApiUrl() + "/authentication/logout/";
    const currentUser = JSON.parse(sessionStorage.currentUser);

    const myHeaders = new HttpHeaders({
      Authorization: "Bearer " + currentUser.jsonWebToken
    });

    return this.httpClient.post(url, currentUser.username, {
      headers: myHeaders
    });
  }
}
