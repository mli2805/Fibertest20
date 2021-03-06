import { DOCUMENT } from "@angular/common";
import { Component, Inject } from "@angular/core";
import { TranslateService } from "@ngx-translate/core";

@Component({
  selector: "ft-root",
  templateUrl: "./app.component.html",
  styleUrls: ["./app.component.css"],
})
export class AppComponent {
  title = "Fibertest";
  private language: string;

  constructor(
    private ts: TranslateService,
    @Inject(DOCUMENT) private doc: Document
  ) {
    document.addEventListener("contextmenu", (event) => event.preventDefault());
    console.log("application c-tor.");

    const lng = sessionStorage.getItem("language");
    if (lng === null) {
      this.language = this.ts.currentLang;
      sessionStorage.setItem("language", this.language);
    } else {
      this.language = lng;
      this.ts.use(lng);
    }
    console.log(`current language is ${this.language}`);
  }
}
