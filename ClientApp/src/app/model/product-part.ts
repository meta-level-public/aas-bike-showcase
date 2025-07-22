import { KatalogEintrag } from "./katalog-eintrag";

export interface ProductPart {
  id: number|undefined;
  katalogEintrag: KatalogEintrag;
  name: string;
  price: number;
  amount: number;
  usageDate: Date;

}
