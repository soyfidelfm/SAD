import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SalesGoalCard } from './sales-goal-card';

describe('SalesGoalCard', () => {
  let component: SalesGoalCard;
  let fixture: ComponentFixture<SalesGoalCard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SalesGoalCard]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SalesGoalCard);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
