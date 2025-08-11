import { Routes } from '@angular/router';
import { AssemblyComponent } from './assembly/assembly.component';
import { CatalogImportComponent } from './catalog-import/catalog-import.component';
import { CatalogListComponent } from './catalog-list/catalog-list.component';
import { ConfigurationCreateComponent } from './configuration-create/configuration-create.component';
import { ConfigurationListComponent } from './configuration-list/configuration-list.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { DiscoveryComponent } from './discovery/discovery.component';
import { DppComponent } from './dpp/dpp.component';
import { GoodsIncomingComponent } from './goods-incoming/goods-incoming.component';
import { GoodsListComponent } from './goods-list/goods-list.component';
import { ProductionOrderListComponent } from './production-order-list/production-order-list.component';
import { RegistryComponent } from './registry/registry.component';
import { RepositoryComponent } from './repository/repository.component';
import { SetupComponent } from './setup/setup.component';
export const routes: Routes = [
  { path: '', component: DashboardComponent, pathMatch: 'full' },
  {
    path: 'browse',
    children: [
      { path: 'repository', component: RepositoryComponent },
      { path: 'discovery', component: DiscoveryComponent },
      { path: 'registry', component: RegistryComponent },
    ],
  },
  {
    path: 'catalog',
    children: [
      { path: 'list', component: CatalogListComponent },
      { path: 'import', component: CatalogImportComponent },
    ],
  },
  {
    path: 'config',
    children: [
      { path: 'list', component: ConfigurationListComponent },
      { path: 'create', component: ConfigurationCreateComponent },
    ],
  },
  {
    path: 'production',
    children: [
      { path: 'assembly', component: AssemblyComponent },
      { path: 'assembly/:id', component: AssemblyComponent }
    ],
  },
  {
    path: 'goods',
    children: [
      { path: 'list', component: GoodsListComponent },
      { path: 'incoming', component: GoodsIncomingComponent },
    ],
  },
  {
    path: 'production-orders',
    children: [{ path: 'list', component: ProductionOrderListComponent }],
  },
  {
    path: 'documentation',
    children: [{ path: 'dpp', component: DppComponent }],
  },
  {
    path: 'setup',
    children: [{ path: 'setup', component: SetupComponent }],
  },
];
