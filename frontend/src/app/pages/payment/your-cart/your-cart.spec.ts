import { ComponentFixture, TestBed } from '@angular/core/testing';

import { YourCart } from './your-cart';

describe('YourCart', () => {
  let component: YourCart;
  let fixture: ComponentFixture<YourCart>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [YourCart]
    })
    .compileComponents();

    fixture = TestBed.createComponent(YourCart);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
