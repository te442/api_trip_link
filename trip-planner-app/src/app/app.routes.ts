import { Routes } from '@angular/router';
import { DestinationsListComponent } from './components/destinations-list/destinations-list.component';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { MyTripsComponent } from './components/my-trips/my-trips.component';
import { TripWizardComponent } from './components/trip-wizard/trip-wizard.component';
import { OptimizeScreenComponent } from './components/optimize-screen/optimize-screen.component';
import { TripResultComponent } from './components/trip-result/trip-result.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'my-trips', component: MyTripsComponent, canActivate: [authGuard] },
  { path: 'plan', component: TripWizardComponent, canActivate: [authGuard] },
  { path: 'plan/optimize/:tripId', component: OptimizeScreenComponent, canActivate: [authGuard] },
  { path: 'trips/:id/result', component: TripResultComponent, canActivate: [authGuard] },
  { path: 'destinations', component: DestinationsListComponent, canActivate: [authGuard] },
];
