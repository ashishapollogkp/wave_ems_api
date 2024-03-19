import { Component, OnDestroy, OnInit, EventEmitter, Input, Output } from '@angular/core';
import { AppRoutingModule } from './app-routing.module';
import { Router } from '@angular/router';
//import { FileUploader } from 'ng2-file-upload';
import { DomSanitizer, SafeHtml, SafeUrl, SafeStyle } from '@angular/platform-browser';
import { HttpService } from '../common/service/http.service';
import { HttpRequestObj } from '../common/service/HttpRequestObj';
import { ValidationHandler } from '../common/ValidationHandler';
import { ResponseContainer } from '../common/ResponseContainer';
import { RequestContainer } from '../common/RequestContainer';

import * as Highcharts from 'highcharts';
//import 'chartist-plugin-tooltips';
//declare var tinymce: any;
declare var $: any;
//declare var jQuery: any;

//var path = require('path');

//var _publicPath = path.resolve(__dirname, '../../../../public');

var baseUrl = $('#BaseUrl').data('baseurl');
var baseId = $('#sidudethmc').data('bind');
var baseKey = $('#activKey').data('bind');

//var _templateURL = baseUrl + 'Administrator/Dashboard' ;

@Component({
  templateUrl: './pages/dashboard.component.html',
  providers: [HttpService, ValidationHandler],
  //styleUrls: [_cssURL]
})
export class DashboardComponent implements OnInit {
  public responseData: any = {};
  public donatActuals: any = {};
  public donatActuals1: any = {};
  public donatActuals2: any = {};
  public resDataObj: any = {};
  public empProfileData: any = {};
  public performanceData: any = {};
  public attPerformanceData: any = {};
  public myteam: any = [];
  public ObserveResponse: any = [];
  public categories: any = {};
  public submitObj: any = {};
  public resData: any = {};
  public setCreate: boolean = true;
  public setUpdate: boolean = false;
  @Input() elementId: String;
  @Output() onEditorKeyup = new EventEmitter<any>();
  editor;


  ngOnInit() {

    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/EmployeeDashboard').subscribe(
      responseObj => {

        $('.spinner').css('display', 'none');

        if (responseObj['status'] == "success") {

          this.responseData = responseObj['data'];
          this.empProfileData = this.responseData['employeebasicdetails'];
          //this.performanceData = datas.data;
          if (this.responseData['role'] == "Administrator" || this.responseData['role'] == "Admin") {

            $("#adminid").css("display", "");

            Highcharts.chart('EmpPeiChart', {
              chart: {
                type: 'column'
              },
              title: {
                text: 'Employee Information'
              },
              xAxis: {
                type: 'category',
                title: {
                  text: 'Total employee exist in attendance management'
                }
              },
              yAxis: {
                title: {
                  text: 'Employee'
                }

              },
              legend: {
                enabled: false
              },
              plotOptions: {
                series: {
                  borderWidth: 0,
                  dataLabels: {
                    enabled: true,
                    format: '{point.y:.1f}'
                  }
                }
              },

              tooltip: {
                pointFormat: '<span style="color:{point.color}">{point.name}</span>: <b>{point.y:.2f}</b><br/>'
              },
              series: [{
                type: undefined,
                colorByPoint: true,
                data: [
                  {
                    name: "Total Employee",
                    y: this.responseData['totalemployee'],
                  },
                  {
                    name: "Active Employee",
                    y: this.responseData['activeemployee'],
                  },
                  {
                    name: "In Active Employee",
                    y: this.responseData['inactiveemployee'],
                  }
                ],
              }]
            });

            Highcharts.chart('ApplyLeaveinfo', {
              chart: {
                type: 'column'
              },
              title: {
                text: 'Apply Leave Information'
              },
              xAxis: {
                type: 'category',
                title: {
                  text: 'Total apply leave exist in attendance management'
                }
              },
              yAxis: {
                title: {
                  text: 'Apply Leave'
                }

              },
              legend: {
                enabled: false
              },
              plotOptions: {
                series: {
                  borderWidth: 0,
                  dataLabels: {
                    enabled: true,
                    format: '{point.y:.1f}'
                  }
                }
              },

              tooltip: {
                pointFormat: '<span style="color:{point.color}">{point.name}</span>: <b>{point.y:.2f}</b><br/>'
              },
              series: [{
                type: undefined,
                colorByPoint: true,
                data: [
                  {
                    name: "Total Leave",
                    y: this.responseData['totalleave'],
                  },
                  {
                    name: "Pending Leave",
                    y: this.responseData['pendingleave'],
                  },
                  {
                    name: "Approved Leave",
                    y: this.responseData['approveleave'],
                  },
                  {
                    name: "Rejected Leave",
                    y: this.responseData['rejectleave'],
                  }
                ],
              }]
            });

          }
          else {

            Highcharts.chart('animating-donut', {
              chart: {
                type: 'pie',
                options3d: {
                  enabled: true,
                  alpha: 45
                }
              },
              title: {
                text: 'Leave Performance'
              },
              plotOptions: {
                pie: {
                  innerSize: 100,
                  depth: 45
                }
              },
              series: [{
                type: undefined,
                name: 'Leave',
                data: [
                  ['Total', this.responseData['total']],
                  ['Availd', this.responseData['availd']],
                  ['Balance', this.responseData['balancel']]
                ]
              }]
            });

          }
        }



      });

  }







  // routing
  constructor(

    private router: Router,
    private sanitization: DomSanitizer,
    private httpService: HttpService,
    private validationHandler: ValidationHandler
  ) {

  }

  statusSubmit(targetIndex, status) {
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['status'] = status;
    requestObj['id'] = this.responseData['applyleave'][targetIndex].id;
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;

    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/ApproveOrRejectApplyLeave').subscribe(datas => {
      $('.spinner').css('display', 'none');
      if (datas['status'] == 'success') {
        $("#changesubmit").modal('hide');
        this.ngOnInit();
      }
    });
  }

  // view apply leave request
  viewApplyLeave() {
    this.router.navigate(['/leave-request']);
  }

}
