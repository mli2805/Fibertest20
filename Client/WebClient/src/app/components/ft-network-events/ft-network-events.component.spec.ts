import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtNetworkEventsComponent } from './ft-network-events.component';

describe('FtNetworkEventsComponent', () => {
  let component: FtNetworkEventsComponent;
  let fixture: ComponentFixture<FtNetworkEventsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtNetworkEventsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtNetworkEventsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
