import { Injectable } from "@angular/core";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Utils } from "../Utils/utils";
import { AttachTraceDto } from "../models/dtos/port/attachTraceDto";
import { DetachOtauDto } from "../models/dtos/rtu/detachOtauDto";
import { AttachOtauDto } from "../models/dtos/port/attachOtauDto";

@Injectable({
  providedIn: "root",
})
export class PortApiService {
  constructor(private httpClient: HttpClient) {}

  postRequest(request: string, body: any) {
    const url = Utils.GetWebApiUrl() + `/port/${request}`;
    const currentUser = JSON.parse(sessionStorage.currentUser);

    const myHeaders = new HttpHeaders({
      Authorization: "Bearer " + currentUser.jsonWebToken,
    });
    return this.httpClient.post(url, body, { headers: myHeaders });
  }

  detachTrace(id: string) {
    const url = Utils.GetWebApiUrl() + `/port/detach-trace/${id}`;
    const currentUser = JSON.parse(sessionStorage.currentUser);

    const myHeaders = new HttpHeaders({
      Authorization: "Bearer " + currentUser.jsonWebToken,
    });
    return this.httpClient.post(url, null, { headers: myHeaders });
  }
}
