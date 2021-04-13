import { Injectable } from "@angular/core";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Utils } from "../Utils/utils";
import { AssignBaseRefDtoWithFiles } from "../models/dtos/trace/assignBaseRefDtoWithFiles";
import { formatDate } from "@angular/common";

@Injectable({
  providedIn: "root",
})
export class OneApiService {
  public connectionId: string;

  constructor(private httpClient: HttpClient) {}

  getRequest(request: string, params: any = null) {
    const url = Utils.GetWebApiUrl() + `/${request}`;
    // console.log(`get request with url ${url}`);
    console.log(`[${Utils.stime()}] get request with url ${url}`);
    const currentUser = JSON.parse(sessionStorage.currentUser);

    // now these two parameters are used only by GetTree request (on web api side)
    // just add function parameters on web api side to use these
    if (params === null) {
      params = {
        connectionId: currentUser.connectionId,
        username: currentUser.username,
      };
    }
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

  async getSorAsBlobFromServer(
    isSorFile: boolean, // for sor file - true, for measurement client - false
    sorFileId: number,
    measGuid: string,
    isBase: boolean,
    isInVxSorFormat: boolean // for display - true, for save - false
  ) {
    const url = Utils.GetWebApiUrl() + `/sor/Get-sor-octetstream`;
    const currentUser = JSON.parse(sessionStorage.currentUser);

    const headers = new HttpHeaders().set(
      "Authorization",
      "Bearer " + currentUser.jsonWebToken
    );

    const params = {
      isSorFile: isSorFile.toString(),
      sorFileId: sorFileId.toString(),
      measGuid,
      isBaseIncluded: isBase.toString(),
      isVxSor: isInVxSorFormat.toString(),
    };

    const response = await this.httpClient
      .get(url, { headers, params, responseType: "blob" })
      .toPromise();
    return response;
  }
}
