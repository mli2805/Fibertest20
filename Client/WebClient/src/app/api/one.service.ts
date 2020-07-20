import { Injectable } from "@angular/core";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Utils } from "../Utils/utils";
import { formatDate } from "@angular/common";
import { BaseRefType } from "../models/enums/baseRefType";
import { AssignBaseRefDtoWithFiles } from "../models/dtos/trace/assignBaseRefDtoWithFiles";
import { BaseRefFile } from "../models/underlying/baseRefFile";
import { SorFileDto } from "../models/underlying/sorFileDto";
import { ReturnCode } from "../models/enums/returnCode";
import { VxSorFileDto } from "../models/underlying/vxSorFileDto";

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

  async getSorFileFromServer(sorFileId: number) {
    const res = (await this.getRequest(
      `misc/get-sor-file/${sorFileId}`
    ).toPromise()) as SorFileDto;

    console.log(res);

    if (res.ReturnCode === ReturnCode.Error) {
      alert(`failed to fetch sor file ${sorFileId}!`);
      return null;
    }
    console.log(`received ${res.SorBytes.length} bytes`);
    return res.SorBytes;
  }

  async getVxSorOctetStreamFromServer(sorFileId: number) {
    const url =
      Utils.GetWebApiUrl() + `/misc/Get-vxsor-octetstream/${sorFileId}`;
    console.log(url);
    const currentUser = JSON.parse(sessionStorage.currentUser);

    const headers = new HttpHeaders().set(
      "Authorization",
      "Bearer " + currentUser.jsonWebToken
    );

    const params = { isBase: "true" };

    const response = await this.httpClient
      .get(url, { headers, params, responseType: "blob" })
      .toPromise();
    console.log("response: ");
    console.log(response);

    return response;
  }
}
