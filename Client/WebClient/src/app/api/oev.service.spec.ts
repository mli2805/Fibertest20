import { TestBed } from '@angular/core/testing';

import { OevService } from './oev.service';

describe('OevService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: OevService = TestBed.get(OevService);
    expect(service).toBeTruthy();
  });
});
