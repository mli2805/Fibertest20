import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtOptEventsComponent } from './opt-events.component';

describe('FtOptEventsComponent', () => {
  let component: FtOptEventsComponent;
  let fixture: ComponentFixture<FtOptEventsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtOptEventsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtOptEventsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
