// https://stackblitz.com/edit/simple-dialog-so?file=app%2Fapp.component.ts
// https://blog.vanila.io/just-another-custom-alert-for-angular-c288bebc3c96

import { MatDialog, MAT_DIALOG_DATA } from "@angular/material";
import { FtSimpleDialogComponent } from "./ft-simple-dialog.component";

export class FtMessageBox {
  static show(
    dialog: MatDialog,
    message,
    title = "Alert",
    information = "",
    button = 0,
    allowOutsideClick = false,
    style = 0,
    width = "600px"
  ) {
    const dialogRef = dialog.open(FtSimpleDialogComponent, {
      data: {
        title: title || "Alert",
        message,
        information,
        button: button || 0,
        style: style || 0,
        allow_outside_click: allowOutsideClick || false,
      },
      width,
    });
    return dialogRef.afterClosed();
  }
}

export enum MessageBoxButton {
  Ok = 0,
  OkCancel = 1,
  YesNo = 2,
  AcceptReject = 3,
  Close = 4,
}

export enum MessageBoxStyle {
  Simple = 0,
  Full = 1,
}
