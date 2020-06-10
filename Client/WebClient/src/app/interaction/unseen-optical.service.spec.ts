import { TestBed } from '@angular/core/testing';

import { UnseenOpticalService } from './unseen-optical.service';

describe('UnseenOpticalService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: UnseenOpticalService = TestBed.get(UnseenOpticalService);
    expect(service).toBeTruthy();
  });
});
