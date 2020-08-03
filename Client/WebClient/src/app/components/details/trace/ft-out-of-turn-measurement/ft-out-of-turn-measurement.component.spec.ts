import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtOutOfTurnMeasurementComponent } from './ft-out-of-turn-measurement.component';

describe('FtOutOfTurnMeasurementComponent', () => {
  let component: FtOutOfTurnMeasurementComponent;
  let fixture: ComponentFixture<FtOutOfTurnMeasurementComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtOutOfTurnMeasurementComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtOutOfTurnMeasurementComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
