export class Utils {

  constructor() {}

  static GetWebApiUrl(): string
  {
    const settings = JSON.parse(sessionStorage.settings);
    const protocol = settings.apiProtocol;
    const port = 11080;
    var url = protocol + "://" + window.location.hostname + ":" + port;
    console.log(url);
    return url;
  }

}
