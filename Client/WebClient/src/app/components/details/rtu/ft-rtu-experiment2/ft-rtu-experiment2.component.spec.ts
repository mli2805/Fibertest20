import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtRtuExperiment2Component } from './ft-rtu-experiment2.component';

describe('FtRtuExperiment2Component', () => {
  let component: FtRtuExperiment2Component;
  let fixture: ComponentFixture<FtRtuExperiment2Component>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtRtuExperiment2Component ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtRtuExperiment2Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
