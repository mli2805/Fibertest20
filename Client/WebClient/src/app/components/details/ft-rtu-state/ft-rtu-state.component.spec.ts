import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtRtuStateComponent } from './ft-rtu-state.component';

describe('FtRtuStateComponent', () => {
  let component: FtRtuStateComponent;
  let fixture: ComponentFixture<FtRtuStateComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtRtuStateComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtRtuStateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
