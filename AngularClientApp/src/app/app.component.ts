import { Component, OnInit } from '@angular/core';
import { environment } from 'src/environments/environment';
import { UserDto } from './models/dtos/userDto';
import { LoginInteractionService } from './interactionServices/login/login-interaction.service';

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
      environment.loggedUser = event.loggedUser;
    });
  }

  ngOnInit(): void {
    this.isLogged = false;
  }
}
