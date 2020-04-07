import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtPortMeasurementClientComponent } from './ft-port-measurement-client.component';

describe('FtPortMeasurementClientComponent', () => {
  let component: FtPortMeasurementClientComponent;
  let fixture: ComponentFixture<FtPortMeasurementClientComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtPortMeasurementClientComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtPortMeasurementClientComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
