import { KatalogEintrag } from '../model/katalog-eintrag';

export interface RohteilLookupResult {
  typeKatalogEintrag: KatalogEintrag;
  aasId: string;
  globalAssetId: string;
}
