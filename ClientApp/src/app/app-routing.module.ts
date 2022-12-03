import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { KinesisPageComponent } from './kinesis-page/kinesis-page.component';
import { EventPageComponent } from './event-page/event-page.component';

const routes: Routes = [
  { path: '', redirectTo: 'event', pathMatch: 'full' },
  { path: 'event', component: EventPageComponent },
  { path: 'kinesis', component: KinesisPageComponent },
];

@NgModule({
  imports: [
    RouterModule.forRoot(routes)
  ],
  exports: [RouterModule]
})
export class AppRoutingModule { }
