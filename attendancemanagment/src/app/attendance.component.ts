import { Component, OnDestroy, OnInit, EventEmitter, Input, Output } from '@angular/core';
//import { AppRoutingModule } from './app-routing.module';
import { Router } from '@angular/router';
//import { by, element } from 'protractor';
//import { FileUploader } from 'ng2-file-upload';
import { DomSanitizer, SafeHtml, SafeUrl, SafeStyle } from '@angular/platform-browser';
import { HttpService } from '../common/service/http.service';
import { HttpRequestObj } from '../common/service/HttpRequestObj';
import { ValidationHandler } from '../common/ValidationHandler';
import { ResponseContainer } from '../common/ResponseContainer';
import { RequestContainer } from '../common/RequestContainer';


//declare var tinymce: any;
declare var $: any;

var baseUrl = $('#BaseUrl').data('baseurl');
var baseId = $('#sidudethmc').data('bind');
var baseKey = $('#activKey').data('bind');


@Component({
  templateUrl: './pages/attendance.component.html',
  providers: [HttpService, ValidationHandler],
  //styleUrls: [_cssURL]
})
export class AttendanceComponent implements OnInit {
  public responseData: any = [];
  public attendanceTicket: any = [];
  public attendanceList: any = [];
  public month_names_short: any = [];
  public submitObj: any = {};
  public monthName: any = {};
  public previouMonthName: any = {};
  public previouYear: any = {};
  public sendQuery: any = {};
  public month: any = {};
  public monthDays: any = {};
  public year: any = {};
  public pageNo: any = {};
  public pageSize: any = {};
  public cuYear: string;
  public cuMonth: string;
  public setCreate: any = true;
  public setPageNo: any = false;
  public setPageNoNext: any = false;
  public targetindex: any = {};
  public status_filter: any = {};
  public shiftData: any = [];
  @Input() elementId: String;
  @Output() onEditorKeyup = new EventEmitter<any>();
  editor;


  /*
  * get employee
  */
  ngOnInit() {
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
    this.status_filter.status = "";

    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    this.pageSize = 10;
    requestObj['pageNo'] = 0;
    requestObj['pageSize'] = this.pageSize;
    this.pageNo = requestObj['pageNo'];
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    requestObj['month'] = this.month;
    requestObj['year'] = this.year;
    requestObj['days'] = this.monthDays;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/AttendanceList').subscribe(
      datas => {
        $('.spinner').css('display', 'none');

        if (datas['status'] == "success") {

          if (this.responseData != null) {
            this.responseData = datas['data'];
          }
          else {
            this.responseData = [];
          }
        }
      });

    this.getShift();
  }

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


  // routing
  constructor(

    private router: Router,
    private sanitization: DomSanitizer,
    private httpService: HttpService,
    private validationHandler: ValidationHandler
  ) {

  }


  // apply filter
  ApplyFilter() {
    let status = this.validationHandler.validateDOM('filtersubmit');
    if (!status) {
      $('.spinner').css('display', 'block');
      this.validationHandler.hideErrors("filtersubmit");
      var requestObj = new RequestContainer();
      requestObj['employee_id'] = baseId;
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
            if (this.responseData != null) {
              this.responseData = datas['data'];
            }
            else {
              this.responseData = [];
            }
          }
        });
    }
  }

  /*
  * open model on click
  */
  openModel(obj, date, in_time) {
    this.sendQuery = {}
    this.sendQuery['attendance_id'] = obj;
    this.sendQuery['p_date'] = date;
    this.sendQuery['in_time'] = in_time;
    this.sendQuery['shift'] = "";
    $('#sendquery').modal('show');
  }

  /*
  * send query
  */
  /*
* create submit
*/
  createSubmit() {
    $('.spinner').css('display', 'block');
    let status = this.validationHandler.validateDOM('profileupdateBasic');
    if (!status) {
      $('.spinner').css('display', 'block');
      this.sendQuery['employee_id'] = baseId;
      this.sendQuery['accesskey'] = baseKey;
      //this.sendQuery['out_time'] = this.sendQuery['out_hourse'] + ':' + this.sendQuery['out_minute'];
      this.httpService.postRequest<ResponseContainer>(this.sendQuery, baseUrl + 'api/AttendanceApi/MispunchRequest').subscribe(datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == "success") {
          $('#sendquery').modal('hide');
          this.sendQuery = {};
          this.submitObj = {};
          $('#changesubmit').modal('show');
          this.ngOnInit();
        }
      });
    }
  }


  onFocus() {
    this.validationHandler.hideErrors("profileupdateBasic");
  }


}
