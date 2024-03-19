import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { LeaveRequestComponent } from './leave-request.component';
const routes: Routes = [
    {
        path: '',
        component: LeaveRequestComponent
    }
];

@NgModule({
    
     imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes)
    ],
    declarations: [LeaveRequestComponent]
    //bootstrap: [AttendanceComponent]
    
})
export class LeaveRequestModule {}