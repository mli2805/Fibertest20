import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtRftsEventsComponent } from './ft-rfts-events.component';

describe('FtRftsEventsComponent', () => {
  let component: FtRftsEventsComponent;
  let fixture: ComponentFixture<FtRftsEventsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtRftsEventsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtRftsEventsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
