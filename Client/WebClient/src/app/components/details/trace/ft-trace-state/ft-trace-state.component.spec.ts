import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtTraceStateComponent } from './ft-trace-state.component';

describe('FtTraceStateComponent', () => {
  let component: FtTraceStateComponent;
  let fixture: ComponentFixture<FtTraceStateComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtTraceStateComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtTraceStateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
