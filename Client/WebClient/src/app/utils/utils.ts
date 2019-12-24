export class Utils {
  static GetWebApiUrl(): string {
    const protocol = "http://";
    const port = 11080;
    // const port = 44362;
    return protocol + window.location.hostname + ":" + port;
  }
}
