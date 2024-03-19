import { BrowserModule } from '@angular/platform-browser';
import { APP_BASE_HREF } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
//import { TinymceModule } from 'angular2-tinymce';
//import { Ng4LoadingSpinnerModule } from 'ng4-loading-spinner';
//import { FileSelectDirective, FileDropDirective } from 'ng2-file-upload';
//import { FileUploadModule } from 'ng2-file-upload';
import { AppRoutingModule } from './app-routing.module';
import { HttpClientModule } from '@angular/common/http';
import { HttpService } from '../common/service/http.service';
// html file component
import { AppComponent } from './app.component';
import { DashboardComponent } from './dashboard.component';
import { PayslipComponent } from './payslip.component';

@NgModule({
  declarations: [
        AppComponent, DashboardComponent, PayslipComponent
  ],
  imports: [
    //BrowserModule,
      AppRoutingModule,
      BrowserModule,
      FormsModule,
      HttpModule,
      HttpClientModule,
      //FileUploadModule,
      //NgbModule,
      //TinymceModule.withConfig({
      //    skin_url: baseUrl + 'Content/skins/lightgray',
      //}),
      //Ng4LoadingSpinnerModule.forRoot()
  ],
    providers: [{ provide: APP_BASE_HREF, useValue: '' }, HttpService],
    bootstrap: [AppComponent]
})
export class AppModule { }
