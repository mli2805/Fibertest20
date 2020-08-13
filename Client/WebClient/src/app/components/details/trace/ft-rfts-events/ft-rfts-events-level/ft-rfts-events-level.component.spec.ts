import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtRftsEventsLevelComponent } from './ft-rfts-events-level.component';

describe('FtRftsEventsLevelComponent', () => {
  let component: FtRftsEventsLevelComponent;
  let fixture: ComponentFixture<FtRftsEventsLevelComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtRftsEventsLevelComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtRftsEventsLevelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
