import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SortBy } from './sort-by';

describe('SortBy', () => {
  let component: SortBy;
  let fixture: ComponentFixture<SortBy>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SortBy]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SortBy);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
