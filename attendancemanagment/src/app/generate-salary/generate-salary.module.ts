import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { GenerateSalaryComponent } from './generate-salary.component';
const routes: Routes = [
    {
        path: '',
    component: GenerateSalaryComponent
    }
];

@NgModule({
    
     imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes)
    ],
  declarations: [GenerateSalaryComponent]
    //bootstrap: [AttendanceComponent]
    
})
export class GenerateSalaryModule {}
