import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtRtuNetworkSettingsComponent } from './ft-rtu-network-settings.component';

describe('FtRtuNetworkSettingsComponent', () => {
  let component: FtRtuNetworkSettingsComponent;
  let fixture: ComponentFixture<FtRtuNetworkSettingsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtRtuNetworkSettingsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtRtuNetworkSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
