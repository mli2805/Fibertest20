import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OptEventsComponent } from './opt-events.component';

describe('OptEventsComponent', () => {
  let component: OptEventsComponent;
  let fixture: ComponentFixture<OptEventsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ OptEventsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OptEventsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
