import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { InteractionsParameter } from './interactionsParameter';

@Injectable({
  providedIn: 'root'
})
export class InteractionsService {
  private commandSource = new Subject<InteractionsParameter>();
  commandRecieved$ = this.commandSource.asObservable();

  sendCommand(command: InteractionsParameter) {
    this.commandSource.next(command);
  }
}
