import { HttpClient } from '@angular/common/http';

export class Utils {
  constructor() {}

  static GetWebApiUrl(): string {
    const settings = JSON.parse(sessionStorage.settings);
    const protocol = settings.apiProtocol;
    const port = 11080;
    var url = protocol + "://" + window.location.hostname + ":" + port;
    return url;
  }
  
  static ToLongRussian(timestamp: Date): string {
    const mm = timestamp.getMonth() + 1; // getMonth() is zero-based
    const dd = timestamp.getDate();
  
    const hh = timestamp.getHours();
    const min = timestamp.getMinutes();
    const sec = timestamp.getSeconds();
  
    return [
      (hh > 9 ? "" : "0") + hh,
      ":",
      (min > 9 ? "" : "0") + min,
      ":",
      (sec > 9 ? "" : "0") + sec,
      " ",
      (dd > 9 ? "" : "0") + dd,
      "/",
      (mm > 9 ? "" : "0") + mm,
      "/",
      timestamp.getFullYear(),
    ].join("");
  };

  static toCamel(o) {
    var newO, origKey, newKey, value;
    if (o instanceof Array) {
      return o.map(function (value) {
        if (typeof value === "object") {
          value = this.toCamel(value);
        }
        return value;
      });
    } else {
      newO = {};
      for (origKey in o) {
        if (o.hasOwnProperty(origKey)) {
          newKey = (
            origKey.charAt(0).toLowerCase() + origKey.slice(1) || origKey
          ).toString();
          value = o[origKey];
          if (
            value instanceof Array ||
            (value !== null && value.constructor === Object)
          ) {
            value = this.toCamel(value);
          }
          newO[newKey] = value;
        }
      }
    }
    return newO;
  }

  // Sergey
  // http is a HttpClient injected in constructor
  // public async getFile(fileName: string): Promise<ArrayBuffer> {
  //   const url = `/assets/${fileName}`;
  //   const result = await this.http
  //     .get(url, {
  //       observe: "response",
  //       responseType: "arraybuffer",
  //     })
  //     .toPromise();
  //   return result.body;
  // }
}
