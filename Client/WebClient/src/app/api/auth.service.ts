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
    const body = { username: user, password: pw };

    // const myHeaders = new HttpHeaders({
    //   // "Access-Control-Allow-Origin": "*",
    //   "Access-Control-Allow-Origin": "http://localhost:4200",
    // });

    return this.httpClient.post(url, body);

    // return this.httpClient.post(url, body, {
    //   headers: myHeaders,
    // });
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
