import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { DesignationComponent } from './designation.component';
const routes: Routes = [
    {
        path: '',
        component: DesignationComponent
    }
];

@NgModule({
    
     imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes)
    ],
    declarations: [DesignationComponent]
    //bootstrap: [AttendanceComponent]
    
})
export class DesignationModule {}