import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtTraceInformationComponent } from './trace-information.component';

describe('FtTraceInformationComponent', () => {
  let component: FtTraceInformationComponent;
  let fixture: ComponentFixture<FtTraceInformationComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtTraceInformationComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtTraceInformationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
