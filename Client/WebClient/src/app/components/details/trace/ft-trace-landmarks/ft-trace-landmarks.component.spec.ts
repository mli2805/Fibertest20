import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtTraceLandmarksComponent } from './ft-trace-landmarks.component';

describe('FtTraceLandmarksComponent', () => {
  let component: FtTraceLandmarksComponent;
  let fixture: ComponentFixture<FtTraceLandmarksComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtTraceLandmarksComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtTraceLandmarksComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
