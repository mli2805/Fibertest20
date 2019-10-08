import { Pipe, PipeTransform } from '@angular/core';
import { RtuPartState } from '../models/enums/rtuPartState';

@Pipe({
  name: 'rtuPartStateToUriPipe'
})
export class RtuPartStateUriPipe implements PipeTransform {
  constructor() {}

  transform(value: RtuPartState): string {
    switch (value) {
      case RtuPartState.Broken:
        return './assets/images/RedSquare.png';
      case RtuPartState.Ok:
        return './assets/images/GreenSquare.png';
      case RtuPartState.NotSetYet:
        return './assets/images/EmptySquare.png';
    }
  }
}
