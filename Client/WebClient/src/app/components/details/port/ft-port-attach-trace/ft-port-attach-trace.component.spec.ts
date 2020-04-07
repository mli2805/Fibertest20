import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtPortAttachTraceComponent } from './ft-port-attach-trace.component';

describe('FtPortAttachTraceComponent', () => {
  let component: FtPortAttachTraceComponent;
  let fixture: ComponentFixture<FtPortAttachTraceComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtPortAttachTraceComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtPortAttachTraceComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
