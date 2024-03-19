import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { RoleComponent } from './role.component';
const routes: Routes = [
    {
        path: '',
        component: RoleComponent
    }
];

@NgModule({
    
     imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes)
    ],
    declarations: [RoleComponent]
    //bootstrap: [AttendanceComponent]
    
})
export class RoleModule {}