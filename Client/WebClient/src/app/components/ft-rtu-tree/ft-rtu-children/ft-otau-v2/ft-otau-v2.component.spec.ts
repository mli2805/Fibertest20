import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtOtauV2Component } from './ft-otau-v2.component';

describe('FtOtauV2Component', () => {
  let component: FtOtauV2Component;
  let fixture: ComponentFixture<FtOtauV2Component>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtOtauV2Component ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtOtauV2Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
