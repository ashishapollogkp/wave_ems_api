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
  templateUrl: './pages/employee.component.html',
  providers: [HttpService, ValidationHandler],
  //styleUrls: [_cssURL]
})
export class EmployeeComponent implements OnInit {
  public responseData: any = {};
  public viewList: any = [];
  public submitObj: any = {};
  public requestObj: any = {};
  public pageNo: Number = 0;
  public defaultNo: Number = 0;
  public pageSize: any = {};
  public setCreate: boolean = true;
  public setPageNo: boolean = false;
  public setPageNoNext: boolean = false;
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
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetEmployee').subscribe(
      datas => {
        $('.spinner').css('display', 'none');
        this.responseData = datas['data'];
        //this.viewList = this.responseData.data;
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
  filterSearchData() {
    this.setCreate = true;
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['pageNo'] = 0;
    this.pageNo = requestObj['pageNo'];
    requestObj['pageSize'] = this.pageSize;
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetEmployee').subscribe(
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
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetEmployee').subscribe(
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
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetEmployee').subscribe(
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
      requestObj['pageNo'] = Number(this.pageNo) - 1;
      this.pageNo = requestObj['pageNo'];
    }

    requestObj['pageSize'] = this.pageSize;
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;

    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetEmployee').subscribe(
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

  /*
  * Add new employee
  */
  addSubmit() {
    this.router.navigate(['/add-employee']);
  }

  /*
  * edit employee
  */
  editSubmit(id) {
    this.router.navigate(['/edit-employee', id]);
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

    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/DeleteEmployee').subscribe(datas => {
      $('.spinner').css('display', 'none');
      //if(datas.status == 'success')
      //{
      //	$("#deletesubmit").modal('hide');
      //	this.ngOnInit();
      //}
    });
  }

  downloadMasterData(Obj) {
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['type'] = Obj;
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;

    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/DownloadExcel').subscribe(datas => {
      $('.spinner').css('display', 'none');
      //if(datas.status == 'success')
      //{
      //	//$("#deletesubmit").modal('hide');

      //}
    });
  }


  onFocus() {
    this.validationHandler.hideErrors("profile-submit");
    this.validationHandler.hideErrors("addUserWorkFromPopup");
  }

  // allow work from home
  addWorkFrom(employeeId) {
    this.requestObj.user_id = employeeId;
    this.requestObj.dateList = [{}];
    $("#addUserWorkFromPopup").modal('show');

  }

  // add new list submit

  // add date list
  addDateList() {
    this.requestObj.dateList.push({});
  }

  removeSelectRow(targetIndex) {
    this.requestObj.dateList.splice(targetIndex, 1);
  }

  // add work from home list
  /*
    * create submit
    */
  createSubmit() {
    let status = this.validationHandler.validateDOM('addUserWorkFromPopup');
    if (!status) {
      $('.spinner').css('display', 'block');
      this.requestObj['accesskey'] = baseKey;
      this.requestObj['employee_id'] = baseId;
      this.httpService.postRequest<ResponseContainer>(this.requestObj, baseUrl + 'api/AttendanceApi/CreateWorkFromHomeEmployeeWise').subscribe(datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == 'success') {
          this.requestObj = {};
          $("#addUserWorkFromPopup").modal('hide');
          $("#changesubmit").modal('show');
          
        }
      });
    }

  }


}
