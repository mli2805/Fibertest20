import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SorViewerComponent } from './sor-viewer.component';

describe('SorViewerComponent', () => {
  let component: SorViewerComponent;
  let fixture: ComponentFixture<SorViewerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SorViewerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SorViewerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
