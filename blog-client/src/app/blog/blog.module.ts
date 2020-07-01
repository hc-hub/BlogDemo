import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { BlogRoutingModule } from './blog-routing.module';
import { BlogAppComponent } from '../blog/blog-app.component';
import { MaterialModule } from '../shared/material/material.module';
import { SidenavComponent } from './components/sidenav/sidenav.component';

@NgModule({
  declarations: [
    BlogAppComponent,
    SidenavComponent],
  imports: [
    CommonModule,
    MaterialModule,
    BlogRoutingModule
  ]
})
export class BlogModule { }
