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
  templateUrl: './pages/leave-request.component.html',
  providers: [HttpService, ValidationHandler],
  //styleUrls: [_cssURL]
})
export class LeaveRequestComponent implements OnInit {
  public responseData: any = {};
  public submitObj: any = {};
  public pageNo: any = {};
  public pageSize: any = {};
  public setCreate: any = true;
  public setPageNo: any = false;
  public setPageNoNext: any = false;
  public targetindex: any = {};
  @Input() elementId: String;
  @Output() onEditorKeyup = new EventEmitter<any>();
  editor;


  /*
  * get employee
  */
  ngOnInit() {
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    this.pageSize = 10;
    requestObj['pageNo'] = 0;
    requestObj['pageSize'] = this.pageSize;
    this.pageNo = requestObj['pageNo'];
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetApplyLeaveRequest').subscribe(
      datas => {
        $('.spinner').css('display', 'none');
        this.responseData = datas['data'];
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
  * Filetr
  */
  onChageFilter() {
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['pageNo'] = 0;
    requestObj['pageSize'] = this.pageSize;
    this.pageNo = requestObj['pageNo'];
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetApplyLeaveRequest').subscribe(
      datas => {
        $('.spinner').css('display', 'none');
        this.responseData = datas['data'];
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
      });
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
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetApplyLeaveRequest').subscribe(
      datas => {
        $('.spinner').css('display', 'none');
        this.responseData = datas['data'];
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
      });
  }

  /*
  * pagination next
  */
  nextSubmit() {
    var requestObj = new RequestContainer();
    requestObj['pageNo'] = Number(this.pageNo) + Number(1);
    this.pageNo = requestObj['pageNo'];
    requestObj['pageSize'] = this.pageSize;
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetApplyLeaveRequest').subscribe(
      datas => {
        $('.spinner').css('display', 'none');
        this.responseData = datas['data'];
        if (this.responseData.length == this.pageSize) {
          this.setPageNoNext = true;
          this.setPageNo = true;
        }
        else {
          this.setPageNoNext = false;
          this.setPageNo = true;
        }
      });

  }

  /*
  * pagination previous
  */
  previousSubmit() {
    var requestObj = new RequestContainer();
    if (this.pageNo == 0) {
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

    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetApplyLeaveRequest').subscribe(
      datas => {
        $('.spinner').css('display', 'none');
        this.responseData = datas['data'];
        if (this.responseData.length == this.pageSize) {
          this.setPageNoNext = true;
        }
        else {
          this.setPageNoNext = false;
          this.setPageNo = true;
        }
        if (this.pageNo == 0) {
          this.setPageNo = false;
        }
        else {
          this.setPageNo = true;
        }
      });

  }

  // update status
  statusSubmit(targetIndex, status) {
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['status'] = status;
    requestObj['id'] = this.responseData[targetIndex].id;
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


  onFocus() {
    this.validationHandler.hideErrors("profile-submit");
  }


}
