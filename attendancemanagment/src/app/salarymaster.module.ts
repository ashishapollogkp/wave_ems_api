import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { SalaryMasterComponent } from './salarymaster.component';
const routes: Routes = [
    {
        path: '',
        component: SalaryMasterComponent
    }
];

@NgModule({
    
     imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes)
    ],
    declarations: [SalaryMasterComponent]
    //bootstrap: [AttendanceComponent]
    
})
export class SalaryMasterModule {}