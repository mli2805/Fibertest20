import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtBopEventsComponent } from './ft-bop-events.component';

describe('FtBopEventsComponent', () => {
  let component: FtBopEventsComponent;
  let fixture: ComponentFixture<FtBopEventsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtBopEventsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtBopEventsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
