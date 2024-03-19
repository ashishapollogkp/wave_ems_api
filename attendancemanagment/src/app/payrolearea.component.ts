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
  templateUrl: './pages/payrolearea.component.html',
  providers: [HttpService, ValidationHandler],
  //styleUrls: [_cssURL]
})
export class PayroleAreaComponent implements OnInit {
  public responseData: any = {};
  public targetindex: any = {};
  public keyValueList: any = [];
  public categories: any = {};
  public submitObj: any = {};
  public resData: any = {};
  public tabChange: any = {};
  public setCreate: boolean = true;
  public setUpdate: boolean = false;
  @Input() elementId: String;
  @Output() onEditorKeyup = new EventEmitter<any>();
  editor;


  ngOnInit() {
    $('.spinner').css('display', 'block');
    this.tabChange = "view_id";
    var requestObj = new RequestContainer();
    requestObj['key_type'] = "payrole";
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetCompanyData').subscribe(
      responseObj => {
        $('.spinner').css('display', 'none');
        this.keyValueList = responseObj['data'];
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
      this.submitObj['key_type'] = "payrole";
      this.submitObj['accesskey'] = baseKey;
      this.submitObj['employee_id'] = baseId;
      this.httpService.postRequest<ResponseContainer>(this.submitObj, baseUrl + 'api/AttendanceApi/CreateCompany').subscribe(datas => {
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

  /*
*  update profile
*/
  updateSubmit() {
    let status = this.validationHandler.validateDOM('profileupdateBasic');
    if (!status) {
      $('.spinner').css('display', 'block');
      this.submitObj['accesskey'] = baseKey;
      this.submitObj['employee_id'] = baseId;
      this.httpService.postRequest<ResponseContainer>(this.submitObj, baseUrl + 'api/AttendanceApi/CreateCompany').subscribe(datas => {
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
    this.submitObj = this.keyValueList[Obj];
    this.setCreate = false;
    this.setUpdate = true;
    $('.active').removeClass('active');
    $('.add_new').addClass('active');
    this.tabChange = "add_new";
    $('.spinner').css('display', 'none');
    //var requestObj = new RequestContainer();
    //requestObj['id'] = Obj;
    //requestObj['employee_id'] = baseId;
    //requestObj['accesskey'] = baseKey;
    //this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/ShowKeyValue').subscribe(datas => {
    //    $('.spinner').css('display', 'none');
    //    if (datas['status'] == 'success') {
    //        this.submitObj = datas['data'];
    //        this.setCreate = false;
    //        this.setUpdate = true;
    //    }
    //});
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

    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/DeleteCompany').subscribe(datas => {
      $('.spinner').css('display', 'none');
      if (datas['status'] == 'success') {
        $("#deletesubmit").modal('hide');
        this.ngOnInit();
      }
    });
  }

  changeTabSubmit(obj) {
    $('.active').removeClass('active');
    $('.' + obj).addClass('active');
    this.tabChange = obj;
  }



  onFocus() {
    this.validationHandler.hideErrors("profileupdateBasic");
  }


}
