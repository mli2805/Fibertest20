import { Injectable } from "@angular/core";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Utils } from "../Utils/utils";
import { RtuMonitoringSettingsDto } from "../models/dtos/rtu/rtuMonitoringSettingsDto";

@Injectable({
  providedIn: "root"
})
export class RtuApiService {
  constructor(private httpClient: HttpClient) {}

  getAllRtu() {
    const url = Utils.GetWebApiUrl() + "/rtu";
    const currentUser = JSON.parse(sessionStorage.currentUser);

    const myHeaders = new HttpHeaders({
      Authorization: "Bearer " + currentUser.jsonWebToken
    });
    return this.httpClient.get(url, { headers: myHeaders });
  }

  getOneRtu(id: string, request: string) {
    const url = Utils.GetWebApiUrl() + `/rtu/${request}/${id}`;
    const currentUser = JSON.parse(sessionStorage.currentUser);

    const myHeaders = new HttpHeaders({
      Authorization: "Bearer " + currentUser.jsonWebToken
    });

    return this.httpClient.get(url, { headers: myHeaders });
  }

  postOneRtu(id: string, request: string, body: any) {
    const url = Utils.GetWebApiUrl() + `/rtu/${request}/${id}`;
    const currentUser = JSON.parse(sessionStorage.currentUser);

    const myHeaders = new HttpHeaders({
      Authorization: "Bearer " + currentUser.jsonWebToken
    });

    return this.httpClient.post(url, body, { headers: myHeaders });
  }
}
