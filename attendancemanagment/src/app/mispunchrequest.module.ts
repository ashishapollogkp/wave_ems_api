import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { MispunchRequestComponent } from './mispunchrequest.component';
const routes: Routes = [
    {
        path: '',
        component: MispunchRequestComponent
    }
];

@NgModule({
    
     imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes)
    ],
    declarations: [MispunchRequestComponent]
    //bootstrap: [AttendanceComponent]
    
})
export class MispunchRequestModule {}