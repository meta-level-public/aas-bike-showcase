import { Routes } from '@angular/router';
import { Dpp } from './pages/dpp/dpp';
import { Home } from './pages/home/home';
import { Impressum } from './pages/impressum/impressum';
import { Members } from './pages/members/members';
import { ProjektArchitektur } from './pages/projekt-architektur/projekt-architektur';
import { Projekt } from './pages/projekt/projekt';

export const routes: Routes = [
  { path: '', component: Home },
  { path: 'dpp', component: Dpp },
  { path: 'projekt', component: Projekt },
  { path: 'projekt/architektur', component: ProjektArchitektur },
  { path: 'members', component: Members },
  { path: 'impressum', component: Impressum },
  { path: '**', redirectTo: '' },
];
