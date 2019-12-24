import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtScrollExperimentComponent } from './ft-scroll-experiment.component';

describe('FtScrollExperimentComponent', () => {
  let component: FtScrollExperimentComponent;
  let fixture: ComponentFixture<FtScrollExperimentComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtScrollExperimentComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtScrollExperimentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
