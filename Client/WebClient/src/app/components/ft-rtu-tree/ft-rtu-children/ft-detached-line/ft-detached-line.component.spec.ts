import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtDetachedLineComponent } from './ft-detached-line.component';

describe('FtDetachedLineComponent', () => {
  let component: FtDetachedLineComponent;
  let fixture: ComponentFixture<FtDetachedLineComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtDetachedLineComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtDetachedLineComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
