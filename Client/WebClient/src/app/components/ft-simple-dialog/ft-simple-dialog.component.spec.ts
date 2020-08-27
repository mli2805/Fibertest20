import { async, ComponentFixture, TestBed } from "@angular/core/testing";

import { FtSimpleDialogComponent } from "./ft-simple-dialog.component";

describe("FtSimpleDialogComponent", () => {
  let component: FtSimpleDialogComponent;
  let fixture: ComponentFixture<FtSimpleDialogComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [FtSimpleDialogComponent],
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtSimpleDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
