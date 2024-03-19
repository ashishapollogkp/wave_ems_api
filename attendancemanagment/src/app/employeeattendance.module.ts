import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { EmployeeAttendanceComponent } from './employee-attendance.component';
const routes: Routes = [
    {
        path: '',
        component: EmployeeAttendanceComponent
    }
];

@NgModule({
    
     imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes)
    ],
    declarations: [EmployeeAttendanceComponent]
    //bootstrap: [AttendanceComponent]
    
})
export class EmployeeAttendanceModule {}