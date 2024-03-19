import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { ReportsComponent } from './reports.component';
const routes: Routes = [
    {
        path: '',
        component: ReportsComponent
    }
];

@NgModule({
    
     imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes)
    ],
    declarations: [ReportsComponent]
    //bootstrap: [AttendanceComponent]
    
})
export class ReportsModule {}