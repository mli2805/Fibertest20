import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { LoginEvent } from './loginEvent';

@Injectable({
  providedIn: 'root'
})
export class LoginInteractionService {
  private eventSource = new Subject<LoginEvent>();
  eventReceived$ = this.eventSource.asObservable();

  sendEvent(event: LoginEvent) {
    this.eventSource.next(event);
  }
}
