import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { SummaryComponent } from './summary.component';
const routes: Routes = [
    {
        path: '',
        component: SummaryComponent
    }
];

@NgModule({
    
     imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes)
    ],
    declarations: [SummaryComponent]
    //bootstrap: [AttendanceComponent]
    
})
export class SummaryModule {}