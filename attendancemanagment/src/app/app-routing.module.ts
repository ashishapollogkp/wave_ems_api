import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
// html file component
import { DashboardComponent } from './dashboard.component';
import { PayslipComponent } from './payslip.component';
import { AuthGuard } from './auth.guard';

const routes: Routes = [
    {
        path: '',
        component: DashboardComponent,
        //canActivate: [AuthGuard],
        //pathMatch: 'full',
        runGuardsAndResolvers: 'always'
    },
    {
        path: 'employee',
        loadChildren: () => import('./employee.module').then(mod => mod.EmployeeModule),
        canActivate: [AuthGuard]
    },
    {
        path: 'add-employee',
        loadChildren: () => import('./addemployee.module').then(mod => mod.AddEmployeeModule),
        //canActivate: [AuthGuard]
    },
    {
        path: 'edit-employee/:id',
        loadChildren: () => import('./addemployee.module').then(mod => mod.AddEmployeeModule),
        //canActivate: [AuthGuard]
    },
    {
        path: 'my-team/:team_id',
        loadChildren: () => import('./myteam.module').then(mod => mod.MyTeamModule)
        //canActivate: [AuthGuard]
    },
    {
        path: 'teamattendance',
        loadChildren: () => import('./teamattendance.module').then(mod => mod.TeamAttendanceModule),
        canActivate: [AuthGuard]
    },
    {
        path: 'offerletter',
        loadChildren: () => import('./offerletter.module').then(mod => mod.OfferLetterModule),
        canActivate: [AuthGuard]
    },
    {
        path: 'employee-attendance',
        loadChildren: () => import('./employeeattendance.module').then(mod => mod.EmployeeAttendanceModule),
        canActivate: [AuthGuard]
    },
    {
        path: 'attendance-request',
        loadChildren: () => import('./mispunch.module').then(mod => mod.MispunchModule),
        canActivate: [AuthGuard]
    },
    {
        path: 'approval-mispunch-request',
        loadChildren: () => import('./mispunchrequest.module').then(mod => mod.MispunchRequestModule),
        canActivate: [AuthGuard]
    },
    {
        path: 'profile',
        loadChildren: () => import('./profile.module').then(mod => mod.ProfileModule)
        //canActivate: [AuthGuard]
    },
    {
        path: 'holiday',
        loadChildren: () => import('./holiday.module').then(mod => mod.HolidayModule),
        canActivate: [AuthGuard]
    },
    {
        path: 'role',
        loadChildren: () => import('./role.module').then(mod => mod.RoleModule),
        canActivate: [AuthGuard]
    },
    {
        path: 'leave',
        loadChildren: () => import('./leave.module').then(mod => mod.LeaveModule),
        canActivate: [AuthGuard]
    },
    {
        path: 'shift',
        loadChildren: () => import('./shift.module').then(mod => mod.ShiftModule),
        canActivate: [AuthGuard]
    },
    {
        path: 'department',
        loadChildren: () => import('./department.module').then(mod => mod.DepartmentModule),
        canActivate: [AuthGuard]
    },
    {
        path: 'designation',
        loadChildren: () => import('./designation.module').then(mod => mod.DesignationModule),
        canActivate: [AuthGuard]
    },
    {
        path: 'payrole-area',
        loadChildren: () => import('./payrolearea.module').then(mod => mod.PayroleAreaModule),
        canActivate: [AuthGuard]
    },
    {
        path: 'leave-type',
        loadChildren: () => import('./leavetype.module').then(mod => mod.LeaveTypeModule),
        canActivate: [AuthGuard]
    },
    {
        path: 'reports',
        loadChildren: () => import('./reports.module').then(mod => mod.ReportsModule),
        canActivate: [AuthGuard]
    },
    {
        path: 'summary',
        loadChildren: () => import('./summary.module').then(mod => mod.SummaryModule),
        canActivate: [AuthGuard]
    },
    {
        path: 'leave-request',
        loadChildren: () => import('./leaverequest.module').then(mod => mod.LeaveRequestModule),
        canActivate: [AuthGuard]
    },
    {
        path: 'attendance-summary',
        loadChildren: () => import('./attendance.module').then(mod => mod.AttendanceModule),
        canActivate: [AuthGuard]
    }, 
    {
        path: 'daily-attendance',
        loadChildren: () => import('./dailyattendance.module').then(mod => mod.DailyAttendanceModule),
        canActivate: [AuthGuard]
    },
    {
        path: 'employee-shift',
        loadChildren: () => import('./employeeshift.module').then(mod => mod.EmployeeShiftModule),
        canActivate: [AuthGuard]
    },
    {
        path: 'salary-master',
        loadChildren: () => import('./salarymaster.module').then(mod => mod.SalaryMasterModule),
        canActivate: [AuthGuard]
    },
    {
        path: 'salary-slip',
        loadChildren: () => import('./salaryslip.module').then(mod => mod.SalarySlipModule),
        canActivate: [AuthGuard]
    },
    {
      path: 'generate-salary',
      loadChildren: () => import('./generate-salary/generate-salary.module').then(mod => mod.GenerateSalaryModule),
      canActivate: [AuthGuard]
    },
    {
        path: 'employee-payslip',
        component: PayslipComponent,
        canActivate: [AuthGuard]
    },
    //{ path: '**', redirectTo: 'http://localhost:25182/' }
];

@NgModule({
  //imports: [RouterModule.forRoot(routes)],
  //exports: [RouterModule]
    imports: [RouterModule.forRoot(routes, { useHash: true, onSameUrlNavigation: "reload" })],
    exports: [
        RouterModule
    ],
    declarations: []
})
export class AppRoutingModule { }
