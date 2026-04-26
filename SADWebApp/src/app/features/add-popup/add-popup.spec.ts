import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddPopup } from './add-popup';

describe('AddPopup', () => {
  let component: AddPopup;
  let fixture: ComponentFixture<AddPopup>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddPopup]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddPopup);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
