import { Component, OnDestroy, OnInit, EventEmitter, Input, Output } from '@angular/core';
import { AppRoutingModule } from './app-routing.module';
import { Router, ActivatedRoute } from '@angular/router';
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
  templateUrl: './pages/myteam.component.html',
  providers: [HttpService, ValidationHandler],
  //styleUrls: [_cssURL]
})
export class MyTeamComponent implements OnInit {
  public responseData: any = [];
  public employeeData: any = {};
  public routeUrl: any = {};
  public categories: any = {};
  public errorObjData: any = {};
  public errorObjData1: any = {};
  public errorObjData2: any = {};
  public submitObj: any = {};
  public resData: any = {};
  public attendanceList: any = [];
  public designationList: any = [];
  public companyList: any = [];
  public roleList: any = []
  public id: any = {};
  public setCreate: boolean = true;
  public setUpdate: boolean = false;
  public shiftData: any = [];
  public month: any = {};
  public monthDays: any = {};
  public year: any = {};
  public pageNo: any = {};
  public pageSize: any = {};
  public cuYear: string;
  public cuMonth: string;
  public setPageNo: boolean = false;
  public setPageNoNext: boolean = false;
  public targetindex: any = {};
  public status_filter: any = {};
  public month_names_short: any = [];
  public monthName: any = {};
  @Input() elementId: String;
  @Output() onEditorKeyup = new EventEmitter<any>();
  editor;


  ngOnInit() {
    //$('#from_date').datepicker({
    //    dateFormat: 'dd/mm/yy',
    //});

    //$('#to_date').datepicker({
    //    dateFormat: 'dd/mm/yy',
    //});
    $('#p_date').datepicker({
      dateFormat: 'dd/mm/yyyy',
    });

    $("#from_date").datepicker(
      {
        dateFormat: 'dd/mm/yy',
      });
    $("#to_date").datepicker(
      {
        dateFormat: 'dd/mm/yy',
      });


    var today = new Date();
    var n = today.getDate();
    //var n = 26;
    var dateinmonth = 1;
    this.month = today.getMonth() + 1; //January is 0!
    this.year = today.getFullYear();
    this.month_names_short = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    this.monthName = this.month_names_short[Number(this.month) - 1];

    var cuYear: number = Number(this.year);
    var cuMonth: number = Number(this.month);

    var monthDay = new Date(cuYear, cuMonth, 0).getDate();

    this.monthDays = monthDay;//today.getDaysInMonth(this.year, this.month);

    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    this.pageSize = 10;
    requestObj['pageNo'] = 0;
    requestObj['pageSize'] = this.pageSize;
    this.pageNo = requestObj['pageNo'];
    //requestObj['employee_id'] = this.id;
    requestObj['accesskey'] = baseKey;
    requestObj['month'] = this.month;
    requestObj['year'] = this.year;
    requestObj['days'] = this.monthDays;

    this.id = this.route.snapshot.paramMap.get('team_id');
    this.routeUrl = this.router.url;
    if (this.id != null) {
      $('.spinner').css('display', 'block');
      //var requestObj = new RequestContainer();
      requestObj['employee_id'] = this.id;
      //requestObj['accesskey'] = baseKey;
      this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/AttendanceList').subscribe(datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == 'success') {
          this.attendanceList = datas['data'];
        }
      });
    }

    this.leaveSummary();

  }


  // routing
  constructor(

    private router: Router,
    private sanitization: DomSanitizer,
    private httpService: HttpService,
    private route: ActivatedRoute,
    private validationHandler: ValidationHandler
  ) {

  }

  // leave summary data
  leaveSummary() {

    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['employee_id'] = this.id;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/EmployeeLeaveSummary').subscribe(
      datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == "success") {
          this.responseData = datas['data'];
          console.log(this.responseData);
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

        this.employeeDetails();
      });

  }

  // employee details
  employeeDetails() {
    $('.spinner').css('display', 'block');
    var requestObj1 = new RequestContainer();
    requestObj1['employee_id'] = this.id;
    requestObj1['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj1, baseUrl + 'api/AttendanceApi/EmployeeInfomation').subscribe(
      responseObj => {
        $('.spinner').css('display', 'none');

        if (responseObj['status'] == "success") {
          this.employeeData = responseObj['data'];
        }
      });
  }

  // apply filter
  ApplyFilter() {
    let status = this.validationHandler.validateDOM('filtersubmit');
    if (!status) {
      $('.spinner').css('display', 'block');
      this.validationHandler.hideErrors("filtersubmit");
      var requestObj = new RequestContainer();
      requestObj['employee_id'] = this.id;
      requestObj['accesskey'] = baseKey;
      this.submitObj['from_date'] = $('#from_date').datepicker().val()
      this.submitObj['to_date'] = $('#to_date').datepicker().val()
      requestObj['from_date'] = $('#from_date').datepicker().val();
      requestObj['to_date'] = $('#to_date').datepicker().val();
      requestObj["type"] = this.status_filter['status'];
      this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/AttendanceList').subscribe(
        datas => {
          $('.spinner').css('display', 'none');
          if (datas['status'] == "success") {
            this.attendanceList = datas['data'];
            if (this.attendanceList != null) {
              this.attendanceList = datas['data'];
            }
            else {
              this.attendanceList = [];
            }
          }
        });
    }
  }

  onFocus() {
    this.validationHandler.hideErrors("profileupdateBasic");
  }
}
