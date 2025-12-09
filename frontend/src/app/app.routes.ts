import { Routes } from '@angular/router';
import { authGuard } from './auth/auth.guard';
import { LoginComponent } from './components/login/login.component';
import { InvoicesPageComponent } from './components/invoices-page/invoices-page.component';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'invoices', component: InvoicesPageComponent,
    canActivate: [authGuard] 
  },
  { path: '**', redirectTo: '/login' }
];
