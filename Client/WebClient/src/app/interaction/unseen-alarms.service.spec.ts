import { TestBed } from '@angular/core/testing';

import { UnseenAlarmsService } from './unseen-alarms.service';

describe('UnseenAlarmsService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: UnseenAlarmsService = TestBed.get(UnseenAlarmsService);
    expect(service).toBeTruthy();
  });
});
