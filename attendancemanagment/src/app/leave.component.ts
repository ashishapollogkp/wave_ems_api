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
//declare var tinymce: any;
declare var $: any;

var baseUrl = $('#BaseUrl').data('baseurl');
var baseId = $('#sidudethmc').data('bind');
var baseKey = $('#activKey').data('bind');


@Component({
  templateUrl: './pages/leave.component.html',
  providers: [HttpService, ValidationHandler],
  //styleUrls: [_cssURL]
})
export class LeaveComponent implements OnInit {
  public responseData: any = {};
  public holidayData: any = [];
  public shiftData: any = [];
  public errorData: any = {};
  public leaveType: any = [];
  public durationType: any = [];
  public applyleaveList: any = [];
  public hrsList: any = [];
  public mintList: any = [];
  public submitObj: any = {};
  public pageNo: any = {};
  public pageSize: any = {};
  public SelectType: any = {};
  public setCreate: any = true;
  public setPageNo: any = false;
  public setLeaveTypeFlag: any = false;
  public setPageNoNext: any = false;
  public targetindex: any = {};
  @Input() elementId: String;
  @Output() onEditorKeyup = new EventEmitter<any>();
  editor;


  /*
  * get employee
  */
  ngOnInit() {
    $('#from_date').datepicker({
      dateFormat: 'dd/mm/yy',
    });
    $('#to_date').datepicker({
      dateFormat: 'dd/mm/yy',
    });

    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/EmployeeLeaveSummary').subscribe(
      datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == "success") {
          this.responseData = datas['data'];

          Highcharts.chart('performancebarchart', {
            chart: {
              type: 'column'
            },
            title: {
              text: 'Employee Leave Summary'
            },
            xAxis: {
              categories: this.responseData['bgNames'],
              crosshair: true
            },
            yAxis: {
              min: 0,
              title: {
                text: 'Leave'
              }
            },
            tooltip: {
              headerFormat: '<span style="font-size:10px">{point.key}</span><table>',
              pointFormat: '<tr><td style="color:{series.color};padding:0">{series.name}: </td>' +
                '<td style="padding:0"><b>{point.y:.1f}</b></td></tr>',
              footerFormat: '</table>',
              shared: true,
              useHTML: true
            },
            plotOptions: {
              column: {
                pointPadding: 0.2,
                borderWidth: 0
              }
            },
            series: [{
              type: undefined,
              name: 'Total',
              data: this.responseData['targets']

            }, {
              type: undefined,
              name: 'Availd',
              data: this.responseData['actuals']

            },
            {
              type: undefined,
              name: 'Balance',
              data: this.responseData['balance']

            }]
          });
        }


      });

    this.getLeaveType();

  }


  // routing
  constructor(

    private router: Router,
    private sanitization: DomSanitizer,
    private httpService: HttpService,
    private validationHandler: ValidationHandler
  ) {

  }

  /*
  * get leave type list
  */

  getLeaveType() {
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    requestObj['key_type'] = "leave";
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetKeyValueData').subscribe(datas => {
      $('.spinner').css('display', 'none');

      this.leaveType = datas['data'];

    });

    this.getCurrentYearCalander();
  }

  getDurationType() {

    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    requestObj['key_type'] = "duration";
    requestObj['id'] = this.submitObj['leave_type'];
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetKeyValueData').subscribe(datas => {
      $('.spinner').css('display', 'none');

      if (datas['status'] == "success") {
        this.durationType = datas['data'];
        this.setLeaveTypeFlag = true;
      }

      this.getTime();

    });
  }

  getTime() {

    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetHrsMint').subscribe(datas => {
      $('.spinner').css('display', 'none');
      if (datas['status'] == "success") {
        this.hrsList = datas['hrs'];
        this.mintList = datas['mint'];
      }

    });
  }

  getCurrentYearCalander() {
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetHolidayData').subscribe(datas => {
      $('.spinner').css('display', 'none');
      this.holidayData = datas['data'];

      // full calander
      var today = new Date();
      var dd = today.getDate();
      var mm = today.getMonth() + 1; //January is 0!
      var yyyy = today.getFullYear();

      if (dd < 10) {
        dd = Number(0) + dd;
      }
      if (mm < 10) {
        mm = Number(0) + mm;
      }
      var todaydate = yyyy + '-' + mm + '-' + dd;
      $('#calendar').fullCalendar({
        header: {
          left: 'prev,next today',
          center: 'title',
          right: 'month,agendaWeek,agendaDay,listWeek'
        },
        defaultDate: todaydate,
        editable: true,
        defaultView: 'month', // display business hours
        navLinks: true, // can click day/week names to navigate views
        eventLimit: true, // allow "more" link when too many events
        events: this.holidayData
      });

    });
    this.getShift();
  }

  // get shift
  getShift() {
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetShift').subscribe(datas => {
      $('.spinner').css('display', 'none');
      this.shiftData = datas['data'];
    });

  }





  /*
* create submit
*/
  createSubmit() {
    let status = this.validationHandler.validateDOM('profileupdateBasic');
    if (!status) {
      $('.spinner').css('display', 'block');
      this.submitObj['from_date'] = $('#from_date').datepicker().val();
      this.submitObj['to_date'] = $('#to_date').datepicker().val();
      this.submitObj['employee_code'] = $('#employee_code').val();
      this.submitObj['employee_id'] = baseId;
      this.submitObj['accesskey'] = baseKey;

      if (this.submitObj['leave_type'] == '40') {
        this.submitObj['from_time'] = this.submitObj['from_hourse'] + ":" + this.submitObj['from_minute'];
        this.submitObj['to_time'] = this.submitObj['to_hourse'] + ":" + this.submitObj['to_minute'];
      }

      this.httpService.postRequest<ResponseContainer>(this.submitObj, baseUrl + 'api/AttendanceApi/ApplyLeave').subscribe(datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == 'success') {
          this.submitObj = {};
          $("#changesubmit").modal('show');
          this.ngOnInit();
        }
        else {
          this.errorData = datas;
          $("#deletesubmit").modal('show');
        }
      });
    }
  }

  onFocus() {
    this.validationHandler.hideErrors("profileupdateBasic");
  }

  // search employee 
  typeHeadSubmit() {

    var loginType = this.SelectType;
    $("#employee_code").autocomplete({
      source: function (request, response) {
        $.ajax({
          url: baseUrl + "/Administrator/EmployeeTypehead",
          type: "POST",
          dataType: "json",
          data: { Prefix: request.term, employee_id: baseId, accessKey: baseKey },
          success: function (datas) {
            response($.map(datas, function (item) {
              return { label: item.name, value: item.employee_id };
            }))

          }
        })
      },
      messages: {
        noResults: "", results: ""
      }
    });

  }


}
