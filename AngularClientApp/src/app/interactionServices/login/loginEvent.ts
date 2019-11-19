import { UserDto } from 'src/app/models/dtos/userDto';

export class LoginEvent {
  isLogged: boolean;

  loggedUser: UserDto;

  message: string;
}
