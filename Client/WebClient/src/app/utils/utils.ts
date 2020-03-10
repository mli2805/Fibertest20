export class Utils {
  static GetWebApiUrl(): string {
    const protocol = "https://";
    const port = 11080;
    return protocol + window.location.hostname + ":" + port;
  }
}
