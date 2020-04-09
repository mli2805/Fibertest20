import { Injectable } from "@angular/core";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Utils } from "../Utils/utils";
import { AttachTraceDto } from "../models/dtos/port/attachTraceDto";

@Injectable({
  providedIn: "root",
})
export class PortApiService {
  constructor(private httpClient: HttpClient) {}

  attachTrace(body: AttachTraceDto) {
    const url = Utils.GetWebApiUrl() + "/port/attachTrace";
    const currentUser = JSON.parse(sessionStorage.currentUser);

    const myHeaders = new HttpHeaders({
      Authorization: "Bearer " + currentUser.jsonWebToken,
    });
    return this.httpClient.post(url, body, { headers: myHeaders });
  }

  attachOtau() {}
  measurementClient() {}
}
