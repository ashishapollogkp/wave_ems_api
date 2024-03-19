import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { MyTeamComponent } from './my-team.component';
const routes: Routes = [
    {
        path: '',
        component: MyTeamComponent
    }
];

@NgModule({
    
     imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes)
    ],
    declarations: [MyTeamComponent]
    //bootstrap: [AttendanceComponent]
    
})
export class MyTeamModule {}