import { TestBed } from '@angular/core/testing';

import { LoginInteractionService } from './login-interaction.service';

describe('LoginInteractionService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: LoginInteractionService = TestBed.get(LoginInteractionService);
    expect(service).toBeTruthy();
  });
});
