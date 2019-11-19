import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Utils } from '../Utils/utils';

@Injectable({
  providedIn: 'root'
})
export class LoginService {
  constructor(private httpClient: HttpClient) {}

  login(user: string, pw: string) {
    const url = Utils.GetWebApiUrl() + '/account/login/';
    const body = { username: user, password: pw };

    return this.httpClient.post(url, body);
  }
}
