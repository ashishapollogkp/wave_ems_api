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
  templateUrl: './pages/leave-type.component.html',
  providers: [HttpService, ValidationHandler],
  //styleUrls: [_cssURL]
})
export class LeaveTypeComponent implements OnInit {
  public responseData: any = {};
  public targetindex: any = {};
  public categories: any = {};
  public errorObjData: any = {};
  public keyValueList: any = [];
  public companyList: any = [];
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
    requestObj['key_type'] = "leave";
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetLeaveType').subscribe(
      responseObj => {
        $('.spinner').css('display', 'none');
        if (responseObj['data'] != null) {
          this.keyValueList = responseObj['data'];
        }
        else {
          this.keyValueList = [];
        }
        this.getCompany();
      });
  }

  getCompany() {
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetCompanyData').subscribe(datas => {
      $('.spinner').css('display', 'none');
      this.companyList = datas['data'];
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
      this.httpService.postRequest<ResponseContainer>(this.submitObj, baseUrl + 'api/AttendanceApi/KeyValueData').subscribe(datas => {
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
      this.httpService.postRequest<ResponseContainer>(this.submitObj, baseUrl + 'api/AttendanceApi/UpdateKeyValueData').subscribe(datas => {
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
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['id'] = Obj;
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/ShowKeyValue').subscribe(datas => {
      $('.spinner').css('display', 'none');
      if (datas['status'] == 'success') {
        this.submitObj = datas['data'];
        this.setCreate = false;
        this.setUpdate = true;
      }
    });
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

    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/DeleteKeyValue').subscribe(datas => {
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
