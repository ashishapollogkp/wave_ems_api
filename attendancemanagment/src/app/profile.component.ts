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
  templateUrl: './pages/profile.component.html',
  providers: [HttpService, ValidationHandler],
  //styleUrls: [_cssURL]
})
export class ProfileComponent implements OnInit {
  public responseData: any = {};
  public employeeDetail: any = {};
  public passwordObj: any = {};
  public categories: any = {};
  public submitObj: any = {};
  public resData: any = {};
  public base64textString: any = {};
  public setCreate: boolean = true;
  public setUpdate: boolean = false;
  @Input() elementId: String;
  @Output() onEditorKeyup = new EventEmitter<any>();
  editor;


  ngOnInit() {
    this.resData = "my-profile";
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/EmployeeProfile').subscribe(
      responseObj => {
        $('.spinner').css('display', 'none');
        if (responseObj['status'] == "success") {
          this.submitObj = responseObj['data'];
          this.employeeDetail = this.submitObj;
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
  *  update profile
  */
  updateSubmit() {
    let status = this.validationHandler.validateDOM('profileupdateBasic');
    if (!status) {
      $('.spinner').css('display', 'block');
      this.submitObj['accesskey'] = baseKey;
      this.httpService.postRequest<ResponseContainer>(this.submitObj, baseUrl + 'api/AttendanceApi/UpdateEmployee').subscribe(datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == 'success') {
          $("#changesubmit").modal('show');
        }
        else {
          $("#errorsubmit").modal('show');
        }
      });
    }
  }

  /*
*  update profile
*/
  updatePasswordSubmit() {
    let status = this.validationHandler.validateDOM('updatePassword');
    if (!status) {
      $('.spinner').css('display', 'block');
      this.passwordObj['accesskey'] = baseKey;
      this.passwordObj['employee_id'] = baseId;
      this.httpService.postRequest<ResponseContainer>(this.passwordObj, baseUrl + 'api/AttendanceApi/ChangePassword').subscribe(datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == 'success') {
          $("#changesubmit").modal('show');
          this.passwordObj = {};
        }
        else {
          $("#errorsubmit").modal('show');
        }
      });
    }
  }

  // profile
  profileSubmit(obj) {
    $('.active').removeClass('active');
    $('.' + obj).addClass('active');
    this.resData = obj;
  }


  onFocus() {
    this.validationHandler.hideErrors("profile-submit");
  }

  // upload image



  onUploadChange(evt: any) {
    const file = evt.target.files[0];

    if (file) {
      const reader = new FileReader();

      reader.onload = this.handleReaderLoaded.bind(this);
      reader.readAsBinaryString(file);
    }
  }

  handleReaderLoaded(e) {
    this.base64textString = {};
    this.base64textString['feature_image'] = btoa(e.target.result);
    $('.spinner').css('display', 'block');
    this.base64textString['accesskey'] = baseKey;
    this.base64textString['employee_id'] = baseId;
    this.httpService.postRequest<ResponseContainer>(this.base64textString, baseUrl + 'api/AttendanceApi/UpdateProfile').subscribe(datas => {
      $('.spinner').css('display', 'none');
      if (datas['status'] == 'success') {
        this.submitObj['feature_image'] = "";
        this.submitObj['feature_image'] = datas['data'];
        $("#changesubmit").modal('show');
        this.ngOnInit();
      }
      else {
        $("#errorsubmit").modal('show');
      }
    });
  }


}
