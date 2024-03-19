import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { OfferLetterComponent } from './offerletter.component';
const routes: Routes = [
    {
        path: '',
        component: OfferLetterComponent
    }
];

@NgModule({
    
     imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes)
    ],
    declarations: [OfferLetterComponent]
    //bootstrap: [AttendanceComponent]
    
})
export class OfferLetterModule {}