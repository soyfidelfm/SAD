import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CatalogService } from '../../core/services/catalog.service';
import { CatalogStore } from '../../core/models/catalog-store.model';

@Component({
  selector: 'app-stores',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './stores.html',
  styleUrls: ['./stores.scss']
})
export class StoresComponent implements OnInit {

  stores: CatalogStore[] = [];

  constructor(private catalog: CatalogService) {}

  ngOnInit(): void {
    this.catalog.getStores().subscribe(data => {
      this.stores = data;
    });
  }
}
