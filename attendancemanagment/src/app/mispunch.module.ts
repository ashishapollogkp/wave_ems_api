import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { MispunchComponent } from './mispunch.component';
const routes: Routes = [
    {
        path: '',
        component: MispunchComponent
    }
];

@NgModule({
    
     imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes)
    ],
    declarations: [MispunchComponent]
    //bootstrap: [AttendanceComponent]
    
})
export class MispunchModule {}