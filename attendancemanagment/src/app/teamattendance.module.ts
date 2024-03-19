import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { TeamAttendanceComponent } from './teamattendance.component';
const routes: Routes = [
    {
        path: '',
        component: TeamAttendanceComponent
    }
];

@NgModule({
    
     imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes)
    ],
    declarations: [TeamAttendanceComponent]
    //bootstrap: [AttendanceComponent]
    
})
export class TeamAttendanceModule {}