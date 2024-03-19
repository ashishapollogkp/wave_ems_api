import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { LeaveComponent } from './leave.component';
const routes: Routes = [
    {
        path: '',
        component: LeaveComponent
    }
];

@NgModule({
    
     imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes)
    ],
    declarations: [LeaveComponent]
    //bootstrap: [AttendanceComponent]
    
})
export class LeaveModule {}