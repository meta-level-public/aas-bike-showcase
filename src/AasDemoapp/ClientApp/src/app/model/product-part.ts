import { KatalogEintrag } from './katalog-eintrag';

export interface ProductPart {
  id: number | undefined;
  katalogEintragId: number | undefined;
  katalogEintrag?: KatalogEintrag; // Optional, da durch JsonIgnore möglicherweise nicht verfügbar
  name: string;
  price: number;
  amount: number;
  usageDate: Date;
}
