import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtRtuExperimentComponent } from './ft-rtu-experiment.component';

describe('FtRtuExperimentComponent', () => {
  let component: FtRtuExperimentComponent;
  let fixture: ComponentFixture<FtRtuExperimentComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtRtuExperimentComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtRtuExperimentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
