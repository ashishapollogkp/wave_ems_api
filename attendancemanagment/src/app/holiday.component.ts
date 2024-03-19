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


//declare var tinymce: any;
declare var $: any;

var baseUrl = $('#BaseUrl').data('baseurl');
var baseId = $('#sidudethmc').data('bind');
var baseKey = $('#activKey').data('bind');


@Component({
  templateUrl: './pages/holiday.component.html',
  providers: [HttpService, ValidationHandler],
  //styleUrls: [_cssURL]
})
export class HolidayComponent implements OnInit {
  public responseData: any = [];
  public targetindex: any = {};
  public keyValueList: any = [];
  public categories: any = {};
  public submitObj: any = {};
  public errorObjData: any = {};
  public resData: any = {};
  public pageNo: Number = 0;
  public defaultNo: Number = 0;
  public pageSize: any = {};
  public setCreate: boolean = true;
  public setUpdate: boolean = false;
  @Input() elementId: String;
  @Output() onEditorKeyup = new EventEmitter<any>();
  editor;

  ngOnInit() {
    $("#holidayDate").datepicker(
      {
        dateFormat: 'yy-mm-dd',
      });
    $('.spinner').css('display', 'block');

    var requestObj = new RequestContainer();
    this.pageSize = 10;
    requestObj['pageNo'] = 0;
    requestObj['pageSize'] = this.pageSize;
    this.pageNo = requestObj['pageNo'];
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetHolidayData').subscribe(
      responseObj => {
        $('.spinner').css('display', 'none');
        this.responseData = responseObj['data'];


        // full calander
        var today = new Date();
        var dd = today.getDate();
        var mm = today.getMonth() + 1; //January is 0!
        var yyyy = today.getFullYear();

        if (dd < 10) {
          dd = Number('0') + dd;
        }
        if (mm < 10) {
          mm = Number('0') + mm;
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
          events: this.responseData
        });

        this.getCompanyCode();
      });
  }

  getCompanyCode() {
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    this.pageSize = 10;
    requestObj['pageNo'] = 0;
    requestObj['pageSize'] = this.pageSize;
    this.pageNo = requestObj['pageNo'];
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetCompanyData').subscribe(datas => {
      $('.spinner').css('display', 'none');
      if (datas['status'] == 'success') {
        this.keyValueList = datas['data'];
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

  /*
  * Create Submit
  */
  CreateSubmit() {
    let status = this.validationHandler.validateDOM('profileupdateBasic');
    if (!status) {
      $('.spinner').css('display', 'block');
      this.submitObj['accesskey'] = baseKey;
      this.submitObj['employee_id'] = baseId;
      this.submitObj['holidayDate'] = $('#holidayDate').datepicker().val();
      this.httpService.postRequest<ResponseContainer>(this.submitObj, baseUrl + 'api/AttendanceApi/CreateHoliday').subscribe(datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == 'success') {
          this.submitObj = {};
          $("#changesubmit").modal('show');
          this.ngOnInit();
        }
        else {
          this.errorObjData = datas['alert'];
          this.errorObjData = this.errorObjData['rsBody'];
          this.errorObjData = this.errorObjData['exceptionBlock'];
          this.errorObjData = this.errorObjData['msg'];
          this.validationHandler.displayErrors(this.errorObjData['validationException'], "profileupdateBasic", null);
        }
      });
    }
  }

  /*
  *  update profile
  */
  updateSubmit() {
    let status = this.validationHandler.validateDOM('profileupdateBasic');
    if (!status) {
      $('.spinner').css('display', 'block');
      this.submitObj['accesskey'] = baseKey;
      this.submitObj['employee_id'] = baseId;
      this.submitObj['holidayDate'] = $('#holidayDate').datepicker().val();
      this.httpService.postRequest<ResponseContainer>(this.submitObj, baseUrl + 'api/AttendanceApi/UpdateHoliday').subscribe(datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == 'success') {
          this.submitObj = {};
          this.setCreate = true;
          this.setUpdate = false;
          $("#changesubmit").modal('show');
          this.ngOnInit();
        }
        else {
          this.errorObjData = datas['alert'];
          this.errorObjData = this.errorObjData['rsBody'];
          this.errorObjData = this.errorObjData['exceptionBlock'];
          this.errorObjData = this.errorObjData['msg'];
          this.validationHandler.displayErrors(this.errorObjData['validationException'], "profileupdateBasic", null);
        }
      });
    }
  }

  editSubmit(Obj) {
    //$('.spinner').css('display','block');
    this.submitObj = this.responseData[Obj];
    this.setCreate = false;
    this.setUpdate = true;
  }



  /*
  * delete
  */
  deleteSubmit(empId) {
    this.targetindex = empId;
    $("#deletesubmit").modal('show');
  }

  deleteRow(Obj) {
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['delete_id'] = Obj;
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;

    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/DeleteHoliday').subscribe(datas => {
      $('.spinner').css('display', 'none');
      if (datas['status'] == 'success') {
        $("#deletesubmit").modal('hide');
        this.ngOnInit();
      }
    });
  }

  onFocus() {
    this.validationHandler.hideErrors("profileupdateBasic");
  }


}
