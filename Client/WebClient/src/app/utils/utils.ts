export class Utils {

  constructor() {}

  static GetWebApiUrl(): string {
    const protocol = "http://";
    const port = 11080;
    return protocol + window.location.hostname + ":" + port;
  }

  GetWebApiUrl2(): string {
    const protocol = JSON.parse(sessionStorage.protocol);
    const port = 11080;
    return protocol + window.location.hostname + ":" + port;
  }

}
