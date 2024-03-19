import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { DepartmentComponent } from './department.component';
const routes: Routes = [
    {
        path: '',
        component: DepartmentComponent
    }
];

@NgModule({
    
     imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes)
    ],
    declarations: [DepartmentComponent]
    //bootstrap: [AttendanceComponent]
    
})
export class DepartmentModule {}