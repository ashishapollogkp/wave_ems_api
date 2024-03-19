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
  templateUrl: './pages/role.component.html',
  providers: [HttpService, ValidationHandler],
  //styleUrls: [_cssURL]
})
export class RoleComponent implements OnInit {
  public responseData: any = {};
  public targetindex: any = {};
  public tabChange: any = {};
  public errorObjData: any = {};
  public keyValueList: any = [];
  public categories: any = {};
  public submitObj: any = {};
  public editMenuObj: any = {};
  public editAccMenuObj: any = {};
  public resData: any = {};
  public editSubMenuObj: any = {};
  public editSubMenuObj2: any = {};
  public menuResult: any = [];
  public menuArray: any = [];
  public menMenuList: any = [];
  public companyList: any = [];
  public errorMsg: String;
  public menuValues: String;
  public menuCount: Number = 0;
  public setCreate: any = true;
  public setUpdate: any = false;
  arrayData: any = {};
  dataSubMenu: any = [];
  @Input() elementId: String;
  @Output() onEditorKeyup = new EventEmitter<any>();
  editor;


  ngOnInit() {
    $('.spinner').css('display', 'block');
    this.tabChange = "view_id";
    var requestObj = new RequestContainer();
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    requestObj['key_type'] = "role";
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetKeyValueData').subscribe(
      responseObj => {
        $('.spinner').css('display', 'none');
        if (responseObj['data'] != null) {
          this.keyValueList = responseObj['data'];
        }
        else {
          this.keyValueList = [];
        }
        this.getMenu();
      });
  }

  // get menu list
  getMenu() {
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    requestObj['key_type'] = "role";
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetMenu').subscribe(
      responseObj => {
        $('.spinner').css('display', 'none');
        this.menMenuList = responseObj['data'];
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
      if (this.menuCount == 0) {
        alert("Select at least one menu");
      }
      else {
        $('.spinner').css('display', 'block');
        this.submitObj['key_type'] = "role";
        this.submitObj['accesskey'] = baseKey;
        this.submitObj['employee_id'] = baseId;
        this.submitObj['access_menu'] = this.menuResult;
        this.httpService.postRequest<ResponseContainer>(this.submitObj, baseUrl + 'api/AttendanceApi/CreateAccessMenu').subscribe(datas => {
          $('.spinner').css('display', 'none');
          if (datas['status'] == 'success') {
            $('.active').removeClass('active');
            $('.view_id').addClass('active');
            this.submitObj = {};
            this.menuResult = [];
            this.menuCount = 0;
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
      this.submitObj['access_menu'] = this.menuResult;
      this.httpService.postRequest<ResponseContainer>(this.submitObj, baseUrl + 'api/AttendanceApi/CreateAccessMenu').subscribe(datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == 'success') {
          this.submitObj = {};
          this.setCreate = true;
          this.setUpdate = false;
          $('.active').removeClass('active');
          $('.view_id').addClass('active');
          $("#changesubmit").modal('show');
          this.menuResult = [];
          this.menuCount = 0;
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
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/ShowAccessRole').subscribe(datas => {
      $('.spinner').css('display', 'none');
      if (datas['status'] == 'success') {
        this.submitObj = datas['data'];
        this.menuResult = [];
        if (this.menMenuList.length > 0) {
          for (var i = 0; i < this.menMenuList.length; i++) {

            this.editMenuObj = this.menMenuList[i];
            this.editMenuObj['checked'] = false;
            for (var j = 0; j < this.submitObj['access_menu'].length; j++) {

              this.editAccMenuObj = this.submitObj['access_menu'][j];

              if (this.editMenuObj['id'] == this.editAccMenuObj) {

                const checked = true;
                this.editMenuObj['checked'] = true;
                if (checked) {
                  this.menuResult.push(this.editMenuObj['id']);
                }

                const count = this.menuResult.length;
                this.menuCount = 0;
                this.menuCount = count;
              }
            }

          }
          this.setCreate = false;
          this.setUpdate = true;
          $('.active').removeClass('active');
          $('.add_new').addClass('active');
          this.tabChange = 'add_new';
        }
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

  changeTabSubmit(obj) {
    $('.active').removeClass('active');
    $('.' + obj).addClass('active');
    this.tabChange = obj;
  }

  // select check box
  onChange(branchNamae: string, event) {
    this.errorMsg = "";
    const checked = event.target.checked;

    if (checked) {
      this.menuResult.push(branchNamae);

    } else {
      const index = this.menuResult.indexOf(branchNamae);
      this.menuResult.splice(index, 1);
    }
    this.menuValues = this.menuResult.toString();
    const count = this.menuResult.length;
    this.menuCount = count;
  }


  onFocus() {
    this.validationHandler.hideErrors("profileupdateBasic");
  }


}
