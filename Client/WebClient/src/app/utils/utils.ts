export class Utils {
  constructor() {}

  static GetWebApiUrl(): string {
    const settings = JSON.parse(sessionStorage.settings);
    const protocol = settings.apiProtocol;
    const port = 11080;
    var url = protocol + "://" + window.location.hostname + ":" + port;
    return url;
  }

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
}
