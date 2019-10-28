import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtTraceStatisticsComponent } from './trace-statistics.component';

describe('FtTraceStatisticsComponent', () => {
  let component: FtTraceStatisticsComponent;
  let fixture: ComponentFixture<FtTraceStatisticsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtTraceStatisticsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtTraceStatisticsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
