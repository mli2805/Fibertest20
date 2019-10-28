import { TranslateService } from '@ngx-translate/core';
import { MatPaginatorIntl } from '@angular/material';
import { Inject, Injectable } from '@angular/core';

@Injectable()
export class FtPaginatorLocale extends MatPaginatorIntl {
  constructor(private ts: TranslateService) {
    super();
    console.log('in constructor');
  }
  itemsPerPageLabel = this.ts.instant('SID_Items_per_page');
  nextPageLabel = this.ts.instant('SID_Next_page');
  previousPageLabel = this.ts.instant('SID_Previous_page');

  getRangeLabel = function(page, pageSize, length) {
    console.log('in function');
    const ofStr = this.ts.instant('SID_of');
    if (length === 0 || pageSize === 0) {
      return '0 ' + ofStr + ' ' +  length;
    }
    length = Math.max(length, 0);
    const startIndex = page * pageSize;
    // If the start index exceeds the list length, do not try and fix the end index to the end.
    const endIndex =
      startIndex < length
        ? Math.min(startIndex + pageSize, length)
        : startIndex + pageSize;
    return startIndex + 1 + ' - ' + endIndex + ' ' + ofStr + ' ' + length;
  };
}
