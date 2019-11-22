import { Component, OnInit } from '@angular/core';
import { LoginService } from 'src/app/api/login.service';
import { UserDto } from 'src/app/models/dtos/userDto';
import { environment } from 'src/environments/environment';
import { LoginInteractionService } from 'src/app/interactionServices/login/login-interaction.service';
import { LoginEvent } from 'src/app/interactionServices/login/loginEvent';
import { globalVars } from 'src/app/global-vars';

@Component({
  selector: 'ft-login',
  templateUrl: './ft-login.component.html',
  styleUrls: ['./ft-login.component.scss']
})
export class FtLoginComponent implements OnInit {
  constructor(
    private loginService: LoginService,
    private loginInteracionService: LoginInteractionService
  ) {}

  user: string;
  pw: string;

  ngOnInit() {}

  login() {
    if (
      environment.production === false &&
      this.user === undefined &&
      this.pw === undefined
    ) {
      this.user = 'root';
      this.pw = 'root';
    }

    this.loginService.login(this.user, this.pw).subscribe((res: UserDto) => {
      if (res === null) {
        console.log('Login failed, try again...');
      }
      this.sendEvent(res);
      globalVars.globalVarSet.loggedUser = res;
    });
  }

  sendEvent(res: UserDto) {
    const event = new LoginEvent();
    event.isLogged = true;
    event.loggedUser = res;
    this.loginInteracionService.sendEvent(event);
  }
}
