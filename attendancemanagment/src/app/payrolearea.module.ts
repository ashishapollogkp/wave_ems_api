import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { PayroleAreaComponent } from './payrolearea.component';
const routes: Routes = [
    {
        path: '',
        component: PayroleAreaComponent
    }
];

@NgModule({
    
     imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes)
    ],
    declarations: [PayroleAreaComponent]
    //bootstrap: [AttendanceComponent]
    
})
export class PayroleAreaModule {}