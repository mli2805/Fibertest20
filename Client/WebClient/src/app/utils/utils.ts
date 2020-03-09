export class Utils {
  static GetWebApiUrl(): string {
    const protocol = "https://";
    // const port = 11080;
    const port = 44371;
    return protocol + window.location.hostname + ":" + port;
  }
}
