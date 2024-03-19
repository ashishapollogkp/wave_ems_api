import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { DailyAttendanceComponent } from './daily-attendance.component';
const routes: Routes = [
    {
        path: '',
        component: DailyAttendanceComponent
    }
];

@NgModule({
    
     imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes)
    ],
    declarations: [DailyAttendanceComponent]
    //bootstrap: [AttendanceComponent]
    
})
export class DailyAttendanceModule {}