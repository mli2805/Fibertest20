import { formatDate } from "@angular/common";

export class Utils {
  constructor() {}

  static GetWebApiUrl(): string {
    if (sessionStorage.settings === undefined) {
      const res = `There is no SETTINGS in sessionStorage!!!`;
      console.log(res);
      return res;
    }
    const settings = JSON.parse(sessionStorage.settings);
    const protocol = settings.apiProtocol;
    const port = settings.apiPort;
    return protocol + "://" + window.location.hostname + ":" + port;
  }

  static dtime(): string {
    return formatDate(Date.now(), "dd MMM HH:mm:ss", "en-US");
  }

  static stime(): string {
    return formatDate(Date.now(), "HH:mm:ss", "en-US");
  }

  static timesss(): string {
    return formatDate(Date.now(), "HH:mm:ss:SSS", "en-US");
  }

  static dtLong(timestamp: Date): string {
    return formatDate(timestamp, "HH:mm:ss dd/MM/yyyy", "ru-RU");
  }

  // static ToLongRussian(timestamp: Date): string {
  //   const mm = timestamp.getMonth() + 1; // getMonth() is zero-based
  //   const dd = timestamp.getDate();

  //   const hh = timestamp.getHours();
  //   const min = timestamp.getMinutes();
  //   const sec = timestamp.getSeconds();

  //   return [
  //     (hh > 9 ? "" : "0") + hh,
  //     ":",
  //     (min > 9 ? "" : "0") + min,
  //     ":",
  //     (sec > 9 ? "" : "0") + sec,
  //     " ",
  //     (dd > 9 ? "" : "0") + dd,
  //     "/",
  //     (mm > 9 ? "" : "0") + mm,
  //     "/",
  //     timestamp.getFullYear(),
  //   ].join("");
  // }

  static toCamel(o: any) {
    let newO: any;
    var origKey, newKey, value;
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

  static generateUUID() {
    // Public Domain/MIT
    var d = new Date().getTime(); // Timestamp
    var d2 = (performance && performance.now && performance.now() * 1000) || 0; // Time in microseconds since page-load or 0 if unsupported
    return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(
      /[xy]/g,
      function (c) {
        var r = Math.random() * 16; // random number between 0 and 16
        if (d > 0) {
          // Use timestamp until depleted
          r = (d + r) % 16 | 0;
          d = Math.floor(d / 16);
        } else {
          // Use microseconds since page-load if supported
          r = (d2 + r) % 16 | 0;
          d2 = Math.floor(d2 / 16);
        }
        return (c === "x" ? r : (r & 0x3) | 0x8).toString(16);
      }
    );
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
