import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtLoginComponent } from './ft-login.component';

describe('FtLoginComponent', () => {
  let component: FtLoginComponent;
  let fixture: ComponentFixture<FtLoginComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtLoginComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtLoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
