import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtRtuMonitoringPortsComponent } from './ft-rtu-monitoring-ports.component';

describe('FtRtuMonitoringPortsComponent', () => {
  let component: FtRtuMonitoringPortsComponent;
  let fixture: ComponentFixture<FtRtuMonitoringPortsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtRtuMonitoringPortsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtRtuMonitoringPortsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
