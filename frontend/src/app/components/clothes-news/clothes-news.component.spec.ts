import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ClothesNewsComponent } from './clothes-news.component';

describe('ClothesNewsComponent', () => {
  let component: ClothesNewsComponent;
  let fixture: ComponentFixture<ClothesNewsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ClothesNewsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ClothesNewsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
