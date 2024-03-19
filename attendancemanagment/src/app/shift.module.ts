import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { ShiftComponent } from './shift.component';
const routes: Routes = [
    {
        path: '',
        component: ShiftComponent
    }
];

@NgModule({
    
     imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes)
    ],
    declarations: [ShiftComponent]
    //bootstrap: [AttendanceComponent]
    
})
export class ShiftModule {}