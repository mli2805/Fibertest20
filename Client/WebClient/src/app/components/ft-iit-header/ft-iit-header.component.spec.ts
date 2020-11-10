import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtIitHeaderComponent } from './ft-iit-header.component';

describe('FtIitHeaderComponent', () => {
  let component: FtIitHeaderComponent;
  let fixture: ComponentFixture<FtIitHeaderComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtIitHeaderComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtIitHeaderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
