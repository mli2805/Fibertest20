import { Pipe, PipeTransform } from "@angular/core";
import { FiberState } from "../models/enums/fiberState";

@Pipe({
  name: "fiberStateToUriPipe"
})
export class FiberStateUriPipe implements PipeTransform {
  constructor() {}

  transform(value: FiberState): string {
    switch (value) {
      case FiberState.Unknown:
      case FiberState.NotJoined:
        return "./assets/images/EmptySquare.png";
      case FiberState.Ok:
        return "./assets/images/GreenSquare.png";
      case FiberState.Suspicion:
        return "./assets/images/YellowSquare.png";
      case FiberState.Minor:
        return "./assets/images/MinorSquare.png";
      case FiberState.Major:
        return "./assets/images/FuchsiaSquare.png";
      case FiberState.User:
        return "./assets/images/GreenSquare.png";
      case FiberState.Critical:
      case FiberState.FiberBreak:
      case FiberState.NoFiber:
        return "./assets/images/RedSquare.png";
      default:
        return "./assets/images/EmptySquare.png";
    }
  }
}
