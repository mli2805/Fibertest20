import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtAssignBaseComponent } from './ft-assign-base.component';

describe('FtAssignBaseComponent', () => {
  let component: FtAssignBaseComponent;
  let fixture: ComponentFixture<FtAssignBaseComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtAssignBaseComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtAssignBaseComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
