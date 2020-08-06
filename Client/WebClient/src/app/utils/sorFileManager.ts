import { OneApiService } from "../api/one.service";
import { Router } from "@angular/router";

export class SorFileManager {
  static Show(
    router: Router,
    isSorFile: boolean,
    sorFileId: number,
    measGuid: string,
    isBaseIncluded: boolean,
    traceTitle: string,
    eventRegistrationTimestamp: Date
  ) {
    const filename =
      traceTitle === "meas"
        ? `${traceTitle} - ${SorFileManager.ToFilename(
            new Date(eventRegistrationTimestamp)
          )}`
        : `${traceTitle} - ID${sorFileId} - ${SorFileManager.ToFilename(
            new Date(eventRegistrationTimestamp)
          )}`;
    const dict = {
      isSorFile,
      sorFileId,
      measGuid,
      isBaseIncluded,
      isVxSor: "true",
      filename,
    };
    sessionStorage.setItem("sorFileRequestParams", JSON.stringify(dict));

    const url = router.serializeUrl(router.createUrlTree(["/sor-viewer"]));
    const newTab = window.open(url, "_blank");
    // newTab.document.title = filename;
    // router.navigate(["/sor-viewer"]);
  }

  static async Download(
    oneApiService: OneApiService,
    sorFileId: number,
    isBaseIncluded: boolean,
    traceTitle: string,
    eventRegistrationTimestamp: Date
  ) {
    const blob = await oneApiService.getSorAsBlobFromServer(
      true,
      sorFileId,
      null,
      isBaseIncluded,
      false
    );

    if (blob !== null) {
      const filename = `${traceTitle} - ID${sorFileId} - ${SorFileManager.ToFilename(
        new Date(eventRegistrationTimestamp)
      )}.sor`;
      SorFileManager.Html5Saver(blob, filename);
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
