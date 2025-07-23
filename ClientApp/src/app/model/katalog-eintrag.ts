import { Kategorie } from "../catalog-list/kategorie";
import { InventoryStatus } from "./inventory-status";
import { KatalogEintragTyp } from "./katalog-eintrag-typ";

export interface KatalogEintrag {
  id?: number;
  name: string;
  kategorie: Kategorie;
  aasId: string;
  localAasId: string;
  remoteRepositoryUrl: string;
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
}
