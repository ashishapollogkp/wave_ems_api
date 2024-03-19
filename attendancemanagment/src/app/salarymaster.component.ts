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
  templateUrl: './pages/salarymaster.component.html',
  providers: [HttpService, ValidationHandler],
  //styleUrls: [_cssURL]
})
export class SalaryMasterComponent implements OnInit {
  public responseData: any = {};
  public targetindex: any = {};
  public salaryMasterList: any = [];
  public salarySlipList: any = [];
  public categories: any = {};
  public submitObj: any = {};
  public resData: any = {};
  public tabChange: any = {};
  public companyList: any = [];
  public setPageNo: boolean = false;
  public setPageNoNext: boolean = false;
  public setCreate: boolean = true;
  public setUpdate: boolean = false;
  public setSCreate: boolean = true;
  public setSUpdate: boolean = false;
  public pageNo: Number = 0;
  public defaultNo: Number = 0;
  public pageSize: any = {};
  @Input() elementId: String;
  @Output() onEditorKeyup = new EventEmitter<any>();
  editor;

  ngOnInit() {
    this.tabChange = "view_id";
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    this.pageSize = 10;
    requestObj['pageNo'] = 0;
    requestObj['pageSize'] = this.pageSize;
    this.pageNo = requestObj['pageNo'];
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetSalaryMaster').subscribe(
      datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == "success") {
          this.salaryMasterList = datas['data'];
          //this.viewList = this.responseData.data;
          if (this.salaryMasterList.length == this.pageSize) {
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

        this.getCompany();

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

  // get company
  getCompany() {
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetCompanyData').subscribe(datas => {
      $('.spinner').css('display', 'none');
      if (datas['status'] == "success") {
        this.companyList = datas['data'];
      }
      else {
        this.companyList = [];
      }

      //this.getSalarySlip();

    });

  }

  // get generate salary slip 
  //getSalarySlip() {
  //    $('.spinner').css('display', 'block');
  //    var requestObj = new RequestContainer();
  //    this.pageSize = 10;
  //    requestObj['pageNo'] = 0;
  //    requestObj['pageSize'] = this.pageSize;
  //    this.pageNo = requestObj['pageNo'];
  //    requestObj['employee_id'] = baseId;
  //    requestObj['accesskey'] = baseKey;
  //    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetSalarySlip').subscribe(
  //        datas => {
  //            $('.spinner').css('display', 'none');
  //            if (datas['status'] == "success") {
  //                this.salarySlipList = datas['data'];
  //                //this.viewList = this.responseData.data;
  //                if (this.salarySlipList.length == this.pageSize) {
  //                    this.setPageNoNext = true;
  //                }
  //                else {
  //                    this.setPageNoNext = false;
  //                }
  //                if (this.pageNo == 0) {
  //                    this.setPageNo = false;
  //                }
  //                else {
  //                    this.setPageNo = true;
  //                }
  //            }

  //        });
  //}

  /*
    * Create Submit
    */
  CreateSubmit() {
    let status = this.validationHandler.validateDOM('profileupdateBasic');
    if (!status) {
      $('.spinner').css('display', 'block');
      this.submitObj['key_type'] = "payrole";
      this.submitObj['accesskey'] = baseKey;
      this.submitObj['employee_id'] = baseId;
      this.httpService.postRequest<ResponseContainer>(this.submitObj, baseUrl + 'api/AttendanceApi/CreateSalaryMaster').subscribe(datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == 'success') {
          this.submitObj = {};
          $("#changesubmit").modal('show');
          this.ngOnInit();
        }
        else {
          this.validationHandler.displayErrors(datas['status'], "profileupdateBasic", null);
        }
      });
    }
  }

  // create slary slip
  //CreateSalarySlipSubmit() {
  //    let status = this.validationHandler.validateDOM('profileupdateBasic2');
  //    if (!status) {
  //        $('.spinner').css('display', 'block');
  //        this.submitObj['key_type'] = "payrole";
  //        this.submitObj['accesskey'] = baseKey;
  //        this.submitObj['employee_id'] = baseId;
  //        this.httpService.postRequest<ResponseContainer>(this.submitObj, baseUrl + 'api/AttendanceApi/CreateSalarySlip').subscribe(datas => {
  //            $('.spinner').css('display', 'none');
  //            if (datas['status'] == 'success') {
  //                this.submitObj = {};
  //                $("#changesubmit").modal('show');
  //                this.getSalarySlip();
  //            }
  //            else {
  //                this.validationHandler.displayErrors(datas['status'], "profileupdateBasic2", null);
  //            }
  //        });
  //    }
  //}

  /*
*  update profile
*/
  updateSubmit() {
    let status = this.validationHandler.validateDOM('profileupdateBasic');
    if (!status) {
      $('.spinner').css('display', 'block');
      this.submitObj['accesskey'] = baseKey;
      this.submitObj['employee_id'] = baseId;
      this.httpService.postRequest<ResponseContainer>(this.submitObj, baseUrl + 'api/AttendanceApi/UpdateSalaryMaster').subscribe(datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == 'success') {
          this.submitObj = {};
          this.setCreate = true;
          this.setUpdate = false;
          $('.active').removeClass('active');
          $('.view_id').addClass('active');
          $("#changesubmit").modal('show');

          this.ngOnInit();
        }
        else {
          this.validationHandler.displayErrors(datas['status'], "profileupdateBasic", null);
        }
      });
    }
  }

  editSubmit(Obj) {
    $('.spinner').css('display', 'block');
    this.submitObj = this.salaryMasterList[Obj];
    this.setCreate = false;
    this.setUpdate = true;
    $('.active').removeClass('active');
    $('.add_new').addClass('active');
    this.tabChange = "add_new";
    $('.spinner').css('display', 'none');
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

    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/DeleteSalaryMaster').subscribe(datas => {
      $('.spinner').css('display', 'none');
      if (datas['status'] == 'success') {
        $("#deletesubmit").modal('hide');
        this.ngOnInit();
      }
    });
  }

  // delete salary slip
  //DeleteSlipSubmit(empId) {
  //    this.targetindex = empId;
  //    $("#deletesalarysubmit").modal('show');
  //}

  //deleteSalarySlipRow(Obj) {
  //    $('.spinner').css('display', 'block');
  //    var requestObj = new RequestContainer();
  //    requestObj['delete_id'] = Obj;
  //    requestObj['employee_id'] = baseId;
  //    requestObj['accesskey'] = baseKey;

  //    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/DeleteSalarySlip').subscribe(datas => {
  //        $('.spinner').css('display', 'none');
  //        if (datas['status'] == 'success') {
  //            $("#deletesalarysubmit").modal('hide');
  //            this.getSalarySlip();
  //        }
  //    });
  //}

  changeTabSubmit(obj) {
    $('.active').removeClass('active');
    $('.' + obj).addClass('active');
    this.tabChange = obj;
  }



  onFocus() {
    this.validationHandler.hideErrors("profileupdateBasic");
  }


}
