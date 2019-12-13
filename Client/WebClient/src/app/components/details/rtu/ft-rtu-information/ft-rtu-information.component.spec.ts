import { async, ComponentFixture, TestBed } from "@angular/core/testing";

import { FtRtuInformationComponent } from "./ft-rtu-information.component";

describe("FtRtuInformationComponent", () => {
  let component: FtRtuInformationComponent;
  let fixture: ComponentFixture<FtRtuInformationComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [FtRtuInformationComponent]
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtRtuInformationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
