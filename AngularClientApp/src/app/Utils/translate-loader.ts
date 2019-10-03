
import { TranslateService } from '@ngx-translate/core';
import { Injectable } from '@angular/core';


class LanguagePair {
  constructor(
    public external: string,
    public internal: string) {
  }
}

export class TranslateLoader {
  loadedLanguages: LanguagePair[] = [];
  languages: any;

  constructor(public translateService: TranslateService) {
  }

  init(languages: any) {
    this.languages = languages;

    this.translateService.onLangChange.subscribe(x => this.loadLanguage(x.lang));
    this.translateService.onDefaultLangChange.subscribe(x => this.loadLanguage(x.lang));
  }

  private loadLanguage(language: string) {
    if (this.loadedLanguages.find(x => language === x.external)) { return; }

    const languageToLoad = this.getLanguageToLoad(language);
    if (languageToLoad) {
      this.translateService.setTranslation(languageToLoad.external, this.languages[languageToLoad.internal], true);
      this.loadedLanguages.push(languageToLoad);
    }
  }

  private getLanguageToLoad(language: string): LanguagePair {
    const languageNames = Object.keys(this.languages);

    for (const languageName of languageNames) {
      if (languageName === language
        || this.normalizeLanguageName(languageName) === language
        || languageName === this.normalizeLanguageName(language)) {
        return new LanguagePair(language, languageName);
      }
    }

    return null;
  }

  private normalizeLanguageName(name: string): string {
    const delemiterIndex = name.indexOf('-');
    if (delemiterIndex === -1) { return name; }

    return name.substring(0, delemiterIndex);
  }
}
