import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtBaseRefsComponent } from './ft-base-refs.component';

describe('FtBaseRefsComponent', () => {
  let component: FtBaseRefsComponent;
  let fixture: ComponentFixture<FtBaseRefsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtBaseRefsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtBaseRefsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
