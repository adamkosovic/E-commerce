import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FavoriteProducts } from './favorite-products';

describe('FavoriteProducts', () => {
  let component: FavoriteProducts;
  let fixture: ComponentFixture<FavoriteProducts>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FavoriteProducts]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FavoriteProducts);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
