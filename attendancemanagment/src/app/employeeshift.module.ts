import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { EmployeeShiftComponent } from './employee-shift.component';
const routes: Routes = [
    {
        path: '',
        component: EmployeeShiftComponent
    }
];

@NgModule({
    
     imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes)
    ],
    declarations: [EmployeeShiftComponent]
    //bootstrap: [AttendanceComponent]
    
})
export class EmployeeShiftModule {}