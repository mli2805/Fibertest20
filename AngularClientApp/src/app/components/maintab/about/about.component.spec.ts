import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtAboutComponent } from './about.component';

describe('AboutComponent', () => {
  let component: FtAboutComponent;
  let fixture: ComponentFixture<FtAboutComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtAboutComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtAboutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
