import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { AttendanceComponent } from './attendance.component';
const routes: Routes = [
    {
        path: '',
        component: AttendanceComponent
    }
];

@NgModule({
    
     imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes)
    ],
    declarations: [AttendanceComponent]
    //bootstrap: [AttendanceComponent]
    
})
export class AttendanceModule {}