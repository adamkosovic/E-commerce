import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminProductEdit } from './admin-product-edit';

describe('AdminProductEdit', () => {
  let component: AdminProductEdit;
  let fixture: ComponentFixture<AdminProductEdit>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminProductEdit]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminProductEdit);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
