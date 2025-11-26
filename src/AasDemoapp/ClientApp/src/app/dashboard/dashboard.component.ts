import { Component, OnInit } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { UrlBase } from '../base/url-base';
import { DashboardService } from './dashboard.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  imports: [TranslateModule],
  standalone: true,
})
export class DashboardComponent extends UrlBase implements OnInit {
  countAAS: number = 0;
  countUpdateable: number = 0;
  countConfiguredProducts: number = 0;
  countProducedProducts: number = 0;

  countAasLoading: boolean = false;
  countUpdateableLoading: boolean = false;
  countConfiguredProductsLoading: boolean = false;

  countProducedProductsLoading: boolean = false;

  constructor(private dashboardService: DashboardService) {
    super();
  }

  ngOnInit(): void {
    this.count();
  }

  async count() {
    try {
      this.countAasLoading = true;
      this.countAAS = await this.dashboardService.countAas();
    } finally {
      this.countAasLoading = false;
    }
    try {
      this.countUpdateableLoading = true;
      this.countUpdateable = await this.dashboardService.countUpdateable();
    } finally {
      this.countUpdateableLoading = false;
    }
    try {
      this.countConfiguredProductsLoading = true;
      this.countConfiguredProducts = await this.dashboardService.countConfiguredProducts();
    } finally {
      this.countConfiguredProductsLoading = false;
    }
    try {
      this.countProducedProductsLoading = true;
      this.countProducedProducts = await this.dashboardService.countProducedProducts();
    } finally {
      this.countProducedProductsLoading = false;
    }
  }
}
