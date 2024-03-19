import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { EmployeeComponent } from './employee.component';
const routes: Routes = [
    {
        path: '',
        component: EmployeeComponent
    }
];

@NgModule({
    
     imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes)
    ],
    declarations: [EmployeeComponent]
    //bootstrap: [AttendanceComponent]
    
})
export class EmployeeModule {}