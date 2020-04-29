import { Injectable } from "@angular/core";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Utils } from "../Utils/utils";
import { formatDate } from "@angular/common";
import { BaseRefType } from "../models/enums/baseRefType";
import { AssignBaseRefDtoWithFiles } from "../models/dtos/trace/assignBaseRefDtoWithFiles";
import { BaseRefFile } from "../models/underlying/baseRefFile";

@Injectable({
  providedIn: "root",
})
export class OneApiService {
  constructor(private httpClient: HttpClient) {}

  getRequest(request: string, params = null) {
    const url = Utils.GetWebApiUrl() + `/${request}`;
    const currentUser = JSON.parse(sessionStorage.currentUser);

    const myHttpOptions = {
      headers: {
        Authorization: "Bearer " + currentUser.jsonWebToken,
      },
      params,
    };

    return this.httpClient.get(url, myHttpOptions);
  }

  postRequest(request: string, body: any) {
    const url = Utils.GetWebApiUrl() + `/${request}`;
    const currentUser = JSON.parse(sessionStorage.currentUser);

    const myHeaders = new HttpHeaders({
      Authorization: "Bearer " + currentUser.jsonWebToken,
    });
    return this.httpClient.post(url, body, { headers: myHeaders });
  }

  postFile(request: string, dto: AssignBaseRefDtoWithFiles) {
    const url = Utils.GetWebApiUrl() + `/${request}`;
    const currentUser = JSON.parse(sessionStorage.currentUser);

    const myHeaders = new HttpHeaders({
      Authorization: "Bearer " + currentUser.jsonWebToken,
    });

    const formData = new FormData();
    dto.baseRefs.forEach((baseRef) => {
      if (baseRef.file !== undefined) {
        formData.append(
          baseRef.type.toString(),
          baseRef.file,
          baseRef.file.name
        );
        baseRef.file = null;
      }
    });
    formData.append("dto", JSON.stringify(dto));

    return this.httpClient.post(url, formData, { headers: myHeaders });
  }
}
