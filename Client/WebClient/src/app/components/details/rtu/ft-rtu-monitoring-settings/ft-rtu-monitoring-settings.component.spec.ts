import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtRtuMonitoringSettingsComponent } from './ft-rtu-monitoring-settings.component';

describe('FtRtuMonitoringSettingsComponent', () => {
  let component: FtRtuMonitoringSettingsComponent;
  let fixture: ComponentFixture<FtRtuMonitoringSettingsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtRtuMonitoringSettingsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtRtuMonitoringSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
