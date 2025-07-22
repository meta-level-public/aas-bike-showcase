import { Kategorie } from "../catalog-list/kategorie";
import { InventoryStatus } from "./inventory-status";

export interface KatalogEintrag {
  id?: number;
  name: string;
  kategorie: Kategorie;
  aasId: string;
  remoteRepositoryUrl: string;
  globalAssetId: string;

  inventoryStatus: InventoryStatus;

  katalogEintragTyp: string;

  rating: number;
  price: number;
  image: string;

  amount: number;
  amountToUse: number;

  referencedType?: KatalogEintrag;
}
