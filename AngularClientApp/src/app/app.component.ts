import { Component, OnInit } from '@angular/core';
import { LoginInteractionService } from './interactionServices/login/login-interaction.service';
import { globalVars } from './global-vars';

@Component({
  selector: 'ft-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  isLogged: boolean;

  constructor(private loginInteractionService: LoginInteractionService) {
    loginInteractionService.eventReceived$.subscribe(event => {
      this.isLogged = true;
      globalVars.globalVarSet.loggedUser = event.loggedUser;
    });
  }

  ngOnInit(): void {
    this.isLogged = false;
  }
}
