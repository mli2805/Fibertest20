import { TestBed } from "@angular/core/testing";

import { OneApiService } from "./one.service";

describe("OneService", () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it("should be created", () => {
    const service: OneApiService = TestBed.get(OneApiService);
    expect(service).toBeTruthy();
  });
});
