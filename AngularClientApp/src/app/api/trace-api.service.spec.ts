import { TestBed } from '@angular/core/testing';

import { TraceApiService } from './trace-api.service';

describe('TraceApiService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: TraceApiService = TestBed.get(TraceApiService);
    expect(service).toBeTruthy();
  });
});
