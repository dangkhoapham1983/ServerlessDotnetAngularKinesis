import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { KinesisPageComponent } from './kinesis-page.component';

describe('KinesisPageComponent', () => {
  let component: KinesisPageComponent;
  let fixture: ComponentFixture<KinesisPageComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ KinesisPageComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(KinesisPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
