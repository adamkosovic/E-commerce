import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CategoryMenu } from './category-menu';

describe('CategoryMenu', () => {
  let component: CategoryMenu;
  let fixture: ComponentFixture<CategoryMenu>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CategoryMenu]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CategoryMenu);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
