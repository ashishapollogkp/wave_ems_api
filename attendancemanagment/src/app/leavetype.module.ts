import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { LeaveTypeComponent } from './leavetype.component';
const routes: Routes = [
    {
        path: '',
        component: LeaveTypeComponent
    }
];

@NgModule({
    
     imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes)
    ],
    declarations: [LeaveTypeComponent]
    //bootstrap: [AttendanceComponent]
    
})
export class LeaveTypeModule {}