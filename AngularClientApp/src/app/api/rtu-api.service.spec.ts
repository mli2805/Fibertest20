import { TestBed } from '@angular/core/testing';

import { RtuApiService } from './rtu-api.service';

describe('RtuApiService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: RtuApiService = TestBed.get(RtuApiService);
    expect(service).toBeTruthy();
  });
});
