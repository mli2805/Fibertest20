import { TestBed } from '@angular/core/testing';

import { OptEvService } from './oev-api.service';

describe('OevApiService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: OptEvService = TestBed.get(OptEvService);
    expect(service).toBeTruthy();
  });
});
