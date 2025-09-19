import { Kategorie } from '../catalog-list/kategorie';
import { InventoryStatus } from './inventory-status';
import { KatalogEintragTyp } from './katalog-eintrag-typ';
import { Supplier } from './supplier';

export interface KatalogEintrag {
  id?: number;
  name: string;
  kategorie: Kategorie;
  aasId: string;
  localAasId: string;
  remoteRepositoryUrl: string; // hier besser auf die Konfiguration verweisen, da wir darüber die securoty Settings haben
  globalAssetId: string;
  inventoryStatus: InventoryStatus;
  katalogEintragTyp: KatalogEintragTyp;
  rating: number;
  price: number;
  image: string;
  amount: number;
  amountToUse: number;
  referencedTypeId?: number;
  referencedType?: KatalogEintrag;
  supplier: Supplier;
}
