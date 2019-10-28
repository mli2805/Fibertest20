import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtTreeDetailsComponent } from './tree-details.component';

describe('FtTreeDetailsComponent', () => {
  let component: FtTreeDetailsComponent;
  let fixture: ComponentFixture<FtTreeDetailsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtTreeDetailsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtTreeDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
