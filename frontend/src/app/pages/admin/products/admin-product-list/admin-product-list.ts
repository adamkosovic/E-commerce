import { Component, OnInit } from '@angular/core';
import { Observable, map } from 'rxjs';
import { Product } from '../../../../models/product/product.model';
import { ProductService, ProductPayload} from '../../../../product.service';
import { CommonModule, AsyncPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChangeDetectorRef } from '@angular/core';

@Component({
  selector: 'app-admin-product-list',
  standalone: true,
  imports: [CommonModule , AsyncPipe, FormsModule],
  templateUrl: './admin-product-list.html',
  styleUrl: './admin-product-list.css'
})
export class AdminProductList implements OnInit {
  products$!: Observable<Product[]>;

  //state för Formulär
  isEditing = false;
  isCreating = false;
  selectedProductId: string | null = null;

  formModel: ProductPayload = {
    title: '',
    description: '',
    price: 0,
    imageUrl: ''
  };

  constructor(private productService: ProductService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.products$ = this.productService.getAll().pipe(
      map(products => [...products].reverse())
    );
  }

  // klick på "+ Ny produkt"
  startCreate(): void {
    this.isCreating = true;
    this.isEditing = false;
    this.selectedProductId = null;
    this.formModel = {
      title: '',
      description: '',
      price: 0,
      imageUrl: ''
    };
  }

  // klick på "Redigera"
  startEdit(p: Product): void {
    this.isEditing = true;
    this.isCreating = false;
    this.selectedProductId = p.id;  
    this.formModel = {
      title: p.title,
      description: p.description,
      price: p.price,
      imageUrl: p.imageUrl
    };
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  // klick på "Ta bort"
  delete(p: Product): void {
    if (!confirm(`Vill du ta bort "${p.title}"?`)) return;

    this.productService.delete(p.id).subscribe({
      next: () => {
        this.loadProducts(),
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Delete failed', err)
    });
  }

  // submit på formuläret
  submitForm(): void {
    if (this.isCreating) {
      this.productService.create(this.formModel).subscribe({
        next: () => {
          this.loadProducts();
          this.isCreating = false;
          this.resetForm();
          this.cdr.detectChanges();
        },
        error: (err) => console.error('Create failed', err)
      });
    } else if (this.isEditing && this.selectedProductId) {
      this.productService.update(this.selectedProductId, this.formModel).subscribe({
        next: () => {
          this.loadProducts();
          this.isEditing = false;
          this.resetForm();
          this.cdr.detectChanges();
        },
        error: (err) => console.error('Update failed', err)
      });
    }
  }

  cancelForm(): void {
    this.isEditing = false;
    this.isCreating = false;
    this.resetForm();
  }

  private resetForm(): void {
    this.selectedProductId = null;
    this.formModel = {
      title: '',
      description: '',
      price: 0,
      imageUrl: ''
    };
  }
}