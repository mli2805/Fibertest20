import { Component, Inject } from "@angular/core";
import { MAT_DIALOG_DATA, MatDialogRef } from "@angular/material/dialog";

@Component({
  selector: "ft-simple-dialog",
  templateUrl: "./ft-simple-dialog.component.html",
  styleUrls: ["./ft-simple-dialog.component.css"],
})
export class FtSimpleDialogComponent {
  style: number;
  title: string;
  message: string;
  information: string;
  button: number;
  allowOutsideClick: boolean;
  constructor(
    public dialogRef: MatDialogRef<FtSimpleDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    console.log(data);
    this.style = data.style || 0;
    this.title = data.title;
    this.message = data.message;
    this.information = data.information;
    this.button = data.button;
    this.dialogRef.disableClose = !data.allow_outside_click || false;
  }
  onOk() {
    this.dialogRef.close({ result: "ok" });
  }
  onCancel() {
    this.dialogRef.close({ result: "cancel" });
  }
  onYes() {
    this.dialogRef.close({ result: "yes" });
  }
  onNo() {
    this.dialogRef.close({ result: "no" });
  }
  onAccept() {
    this.dialogRef.close({ result: "accept" });
  }
  onReject() {
    this.dialogRef.close({ result: "reject" });
  }
  onClose() {
    this.dialogRef.close({ result: "close" });
  }
}
