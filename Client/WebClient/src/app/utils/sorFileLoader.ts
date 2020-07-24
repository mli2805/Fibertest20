import { OneApiService } from "../api/one.service";

export class SorFileLoader {
  static async Download(
    oneApiService: OneApiService,
    sorFileId: number,
    isBaseIncluded: boolean,
    traceTitle: string,
    eventRegistrationTimestamp: Date
  ) {
    const blob = await oneApiService.getSorAsBlobFromServer(
      sorFileId,
      isBaseIncluded,
      false
    );

    if (blob !== null) {
      const filename = `${traceTitle} - ID${sorFileId} - ${SorFileLoader.ToFilename(
        new Date(eventRegistrationTimestamp)
      )}.sor`;
      SorFileLoader.Html5Saver(blob, filename);
    }
  }

  // (C) not mine.
  static Html5Saver(blob, fileName) {
    // to emulate click action
    // because we cannot save directly to client's computer due to security constraints
    const a = document.createElement("a");
    document.body.appendChild(a);
    // a.style = "display: none";
    const url = window.URL.createObjectURL(blob);
    a.href = url;
    a.download = fileName;
    a.click();

    document.body.removeChild(a);
  }

  static ToFilename(timestamp: Date): string {
    const mm = timestamp.getMonth() + 1; // getMonth() is zero-based
    const dd = timestamp.getDate();

    const hh = timestamp.getHours();
    const min = timestamp.getMinutes();
    const sec = timestamp.getSeconds();

    return [
      (dd > 9 ? "" : "0") + dd,
      "-",
      (mm > 9 ? "" : "0") + mm,
      "-",
      timestamp.getFullYear(),
      "-",
      (hh > 9 ? "" : "0") + hh,
      "-",
      (min > 9 ? "" : "0") + min,
      "-",
      (sec > 9 ? "" : "0") + sec,
    ].join("");
  }
}
