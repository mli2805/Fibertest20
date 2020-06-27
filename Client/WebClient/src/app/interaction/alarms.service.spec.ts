import { TestBed } from "@angular/core/testing";

import { AlarmsService } from "./alarms.service";

describe("UnseenAlarmsService", () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it("should be created", () => {
    const service: AlarmsService = TestBed.get(AlarmsService);
    expect(service).toBeTruthy();
  });
});
