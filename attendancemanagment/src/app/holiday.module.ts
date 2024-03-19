import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { HolidayComponent } from './holiday.component';
const routes: Routes = [
    {
        path: '',
        component: HolidayComponent
    }
];

@NgModule({
    
     imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes)
    ],
    declarations: [HolidayComponent]
    //bootstrap: [AttendanceComponent]
    
})
export class HolidayModule {}