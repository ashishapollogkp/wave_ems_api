import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { SalarySlipComponent } from './salaryslip.component';
const routes: Routes = [
    {
        path: '',
        component: SalarySlipComponent
    }
];

@NgModule({
    
     imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes)
    ],
    declarations: [SalarySlipComponent]
    //bootstrap: [AttendanceComponent]
    
})
export class SalarySlipModule {}