import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { ProfileComponent } from './profile.component';
const routes: Routes = [
    {
        path: '',
        component: ProfileComponent
    }
];

@NgModule({
    
     imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes)
    ],
    declarations: [ProfileComponent]
    //bootstrap: [AttendanceComponent]
    
})
export class ProfileModule {}