import { TestBed } from '@angular/core/testing';

import { RtuService } from './rtu.service';

describe('RtuService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: RtuService = TestBed.get(RtuService);
    expect(service).toBeTruthy();
  });
});
