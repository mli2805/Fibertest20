import { Pipe, PipeTransform } from "@angular/core";

@Pipe({
  name: "booleanUri"
})
export class BooleanUriPipe implements PipeTransform {
  transform(value: boolean): string {
    if (value) {
      return "./assets/images/GreenSquare.png";
    } else {
      return "./assets/images/RedSquare.png";
    }

    return "./assets/images/EmptySquare.png";
  }
}
