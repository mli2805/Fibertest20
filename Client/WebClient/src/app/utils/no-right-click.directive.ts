import { HostListener, Directive } from "@angular/core";

@Directive({
  selector: "[ftNoRightClick]",
})
export class NoRightClickDirective {
  @HostListener("contextmenu", ["$event"]) onRightClick(event) {
    event.preventDefault();
  }
}
