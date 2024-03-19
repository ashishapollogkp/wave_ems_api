import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { AddEmployeeComponent } from './add-employee.component';
const routes: Routes = [
    {
        path: '',
        component: AddEmployeeComponent
    }
];

@NgModule({
    
     imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes)
    ],
    declarations: [AddEmployeeComponent]
    //bootstrap: [AttendanceComponent]
    
})
export class AddEmployeeModule {}