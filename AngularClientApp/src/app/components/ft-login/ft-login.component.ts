import { Component, OnInit } from '@angular/core';
import { LoginService } from 'src/app/api/login.service';

@Component({
  selector: 'ft-login',
  templateUrl: './ft-login.component.html',
  styleUrls: ['./ft-login.component.scss']
})
export class FtLoginComponent implements OnInit {
  constructor(private loginServive: LoginService) {}

  user: string;
  pw: string;

  ngOnInit() {}

  login() {
    console.log('button login', this.user, this.pw);
    this.loginServive.login(this.user, this.pw).subscribe(res => console.log(res));
  }
}
