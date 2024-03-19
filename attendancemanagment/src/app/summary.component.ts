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
  templateUrl: './pages/summary.component.html',
  providers: [HttpService, ValidationHandler],
  //styleUrls: [_cssURL]
})
export class SummaryComponent implements OnInit {
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
  public tabChange: any = {};
  public year: any = {};
  // leave request
  public pageNo: Number = 0;
  public pageSize: any = {};
  // mispunch
  public pageNoM: Number = 0;
  public pageSizeM: Number = 10;
  public setCreate: boolean = true;
  // leave request
  public setPageNo: boolean = false;
  public setPageNoNext: boolean = false;
  // mispunch
  public setPageNoM: boolean = false;
  public setPageNoNextM: boolean = false;
  public targetindexM: any = {};
  public targetindex: any = {};
  public status_filter: any = {};
  public mispunchData: any = [];
  @Input() elementId: String;
  @Output() onEditorKeyup = new EventEmitter<any>();
  editor;


  /*
  * get employee
  */
  ngOnInit() {

    $('.spinner').css('display', 'block');
    this.tabChange = "viewLeave";
    var requestObj = new RequestContainer();
    this.pageSize = 10;
    requestObj['pageNo'] = 0;
    requestObj['pageSize'] = this.pageSize;
    this.pageNo = requestObj['pageNo'];
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetApplyLeave').subscribe(
      datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == "success") {
          this.responseData = datas['data'];
          if (this.responseData != null) {
            if (this.responseData.length == this.pageSize) {
              this.setPageNoNext = true;
            }
            else {
              this.setPageNoNext = false;
            }
            if (this.pageNo == 0) {
              this.setPageNo = false;
            }
            else {
              this.setPageNo = true;
            }
          }
          else {
            this.responseData = [];
          }
        }

        this.getMispunch();

      });
  }


  // get mispunch request
  getMispunch() {

    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['pageNo'] = this.pageNoM;
    requestObj['pageSize'] = this.pageSizeM;
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetMispunchRequest').subscribe(
      datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == "success") {
          this.mispunchData = datas['data'];
          if (this.mispunchData != null) {
            if (this.mispunchData.length == this.pageSizeM) {
              this.setPageNoNextM = true;
            }
            else {
              this.setPageNoNextM = false;
            }
            if (this.pageNoM == 0) {
              this.setPageNoM = false;
            }
            else {
              this.setPageNoM = true;
            }
          }
          else {
            this.mispunchData = [];
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

  /*
  * search employee
  */
  searchSubmit() {
    this.setCreate = true;
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['pageNo'] = 0;
    requestObj['search_result'] = this.submitObj['search_result'];
    this.pageNo = requestObj['pageNo'];
    requestObj['pageSize'] = this.pageSize;
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetApplyLeave').subscribe(
      datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == "success") {
          this.responseData = datas['data'];
          if (this.responseData != null) {
            if (this.responseData.length == this.pageSize) {
              this.setPageNoNext = true;
            }
            else {
              this.setPageNoNext = false;
            }
            if (this.pageNo == 0) {
              this.setPageNo = false;
            }
            else {
              this.setPageNo = true;
            }
          }
          else {
            this.responseData = [];
          }
        }
      });
  }

  // search mispunch
  searchMSubmit() {
    this.setCreate = true;
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['pageNo'] = 0;
    requestObj['search_result'] = this.submitObj['searchM_result'];
    this.pageNo = requestObj['pageNo'];
    requestObj['pageSize'] = this.pageSize;
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetMispunchRequest').subscribe(
      datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == "success") {
          this.mispunchData = datas['data'];
          if (this.mispunchData != null) {
            if (this.mispunchData.length == this.pageSizeM) {
              this.setPageNoNextM = true;
            }
            else {
              this.setPageNoNextM = false;
            }
            if (this.pageNoM == 0) {
              this.setPageNoM = false;
            }
            else {
              this.setPageNoM = true;
            }
          }
          else {
            this.mispunchData = [];
          }
        }
      });
  }


  /*
  * pagination next
  */
  nextSubmit() {
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['pageNo'] = Number(this.pageNo) + Number(1);
    this.pageNo = requestObj['pageNo'];
    requestObj['pageSize'] = this.pageSize;
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetApplyLeave').subscribe(
      datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == "success") {
          this.responseData = datas['data'];
          if (this.responseData != null) {
            if (this.responseData.length == this.pageSize) {
              this.setPageNoNext = true;
            }
            else {
              this.setPageNoNext = false;
            }
            if (this.pageNo == 0) {
              this.setPageNo = false;
            }
            else {
              this.setPageNo = true;
            }
          }
          else {
            this.responseData = [];
          }
        }
      });

  }

  // next mispunch
  nextMSubmit() {

    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['pageNo'] = Number(this.pageNoM) + Number(1);
    this.pageNoM = requestObj['pageNo'];
    requestObj['pageSize'] = this.pageSizeM;
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetMispunchRequest').subscribe(
      datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == "success") {
          this.mispunchData = datas['data'];
          if (this.mispunchData != null) {
            if (this.mispunchData.length == this.pageSizeM) {
              this.setPageNoNextM = true;
            }
            else {
              this.setPageNoNextM = false;
            }
            if (this.pageNoM == 0) {
              this.setPageNoM = false;
            }
            else {
              this.setPageNoM = true;
            }
          }
          else {
            this.mispunchData = [];
          }
        }
      });

  }



  /*
  * pagination previous
  */
  previousSubmit() {
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    if (this.pageNo == Number(0)) {
      requestObj['pageNo'] = 0;
      this.pageNo = requestObj['pageNo'];
    }
    else {
      requestObj['pageNo'] = Number(this.pageNo) - Number(1);
      this.pageNo = requestObj['pageNo'];
    }
    requestObj['pageSize'] = this.pageSize;
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;

    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetApplyLeave').subscribe(
      datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == "success") {
          this.responseData = datas['data'];
          if (this.responseData != null) {
            if (this.responseData.length == this.pageSize) {
              this.setPageNoNext = true;
            }
            else {
              this.setPageNoNext = false;
            }
            if (this.pageNo == 0) {
              this.setPageNo = false;
            }
            else {
              this.setPageNo = true;
            }
          }
          else {
            this.responseData = [];
          }
        }
      });

  }

  // mispunch previous
  previousMSubmit() {
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    if (this.pageNoM == Number(0)) {
      requestObj['pageNo'] = 0;
      this.pageNoM = requestObj['pageNo'];
    }
    else {
      requestObj['pageNo'] = Number(this.pageNoM) - Number(1);
      this.pageNoM = requestObj['pageNo'];
    }
    requestObj['pageSize'] = this.pageSizeM;
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;

    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetMispunchRequest').subscribe(
      datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == "success") {
          this.mispunchData = datas['data'];
          if (this.mispunchData != null) {
            if (this.mispunchData.length == this.pageSizeM) {
              this.setPageNoNextM = true;
            }
            else {
              this.setPageNoNextM = false;
            }
            if (this.pageNoM == 0) {
              this.setPageNoM = false;
            }
            else {
              this.setPageNoM = true;
            }
          }
          else {
            this.mispunchData = [];
          }
        }
      });

  }

  /*
  * delete
  */
  deleteApplyLeavess(empId) {
    this.targetindex = empId;
    $("#deletesubmit").modal('show');
  }

  deleteMApplyLeavess(empId) {
    this.targetindexM = empId;
    $("#deleteMsubmit").modal('show');
  }

  deleteRow(Obj) {
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['delete_id'] = Obj;
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;

    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/DeleteApplyLeave').subscribe(datas => {
      $('.spinner').css('display', 'none');
      if (datas['status'] == 'success') {
        $("#deletesubmit").modal('hide');
        this.ngOnInit();
      }
    });
  }

  deleteMRow(Obj) {
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['delete_id'] = Obj;
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;

    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/DeleteMiapunchRequest').subscribe(datas => {
      $('.spinner').css('display', 'none');
      if (datas['status'] == 'success') {
        $("#deleteMsubmit").modal('hide');
        this.getMispunch();
      }
    });
  }

  changeTabSubmit(obj) {
    $('.active').removeClass('active');
    $('.' + obj).addClass('active');
    this.tabChange = obj;
  }

}
