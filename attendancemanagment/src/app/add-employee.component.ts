import { Component, OnDestroy, OnInit, EventEmitter, Input, Output } from '@angular/core';
import { AppRoutingModule } from './app-routing.module';
import { Router, ActivatedRoute } from '@angular/router';
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
  templateUrl: './pages/add-employee.component.html',
  providers: [HttpService, ValidationHandler],
  //styleUrls: [_cssURL]
})
export class AddEmployeeComponent implements OnInit {
  public responseData: any = {};
  public routeUrl: any = {};
  public categories: any = {};
  public errorObjData: any = {};
  public errorObjData1: any = {};
  public errorObjData2: any = {};
  public submitObj: any = {};
  public resData: any = {};
  public departmentList: any = [];
  public designationList: any = [];
  public companyList: any = [];
  public roleList: any = []
  public id: any = {};
  public setCreate: any = true;
  public setUpdate: any = false;
  public shiftData: any = [];
  @Input() elementId: String;
  @Output() onEditorKeyup = new EventEmitter<any>();
  editor;


  ngOnInit() {
    $('#doj').datepicker({
      dateFormat: 'dd/mm/yy',
      yearRange: "c-10:c+0",
      changeMonth: true,
      changeYear: true,
    });

    $('#dol').datepicker({
      dateFormat: 'dd/mm/yy',
    });

    $('#dob').datepicker({
      dateFormat: 'dd/mm/yy',
      yearRange: "c-40:c+0",
      changeMonth: true,
      changeYear: true,
    });

    this.id = this.route.snapshot.paramMap.get('id');
    this.routeUrl = this.router.url;
    if (this.routeUrl == '/edit-employee/' + this.id) {
      $('.spinner').css('display', 'block');
      var requestObj = new RequestContainer();
      requestObj['id'] = this.id;
      requestObj['employee_id'] = baseId;
      requestObj['accesskey'] = baseKey;
      this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/ShowEmployee').subscribe(datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == 'success') {
          this.submitObj = datas['data'];
          this.setCreate = false;
          this.setUpdate = true;
          this.getMasterData();
        }
      });
    }

    this.getCompany();
  }


  // routing
  constructor(

    private router: Router,
    private sanitization: DomSanitizer,
    private httpService: HttpService,
    private route: ActivatedRoute,
    private validationHandler: ValidationHandler
  ) {

  }

  // get shift
  //getShift() {
  //    $('.spinner').css('display', 'block');
  //    var requestObj = new RequestContainer();
  //    requestObj['employee_id'] = baseId;
  //    requestObj['accesskey'] = baseKey;
  //    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetShift').subscribe(datas => {
  //        $('.spinner').css('display', 'none');
  //        this.shiftData = datas['data'];
  //        this.getDepartment();
  //    });

  //}

  // get depatment
  //getDepartment() {
  //    $('.spinner').css('display', 'block');
  //    var requestObj = new RequestContainer();
  //    requestObj['employee_id'] = baseId;
  //    requestObj['accesskey'] = baseKey;
  //    requestObj['key_type'] = "department";
  //    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetKeyValueData').subscribe(datas => {
  //        $('.spinner').css('display', 'none');
  //        this.departmentList = datas['data'];
  //        this.getDesignation();
  //    });

  //}

  getMasterData() {
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    requestObj['pay_code'] = this.submitObj['pay_code'];
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetMasterData').subscribe(datas => {
      $('.spinner').css('display', 'none');
      if (datas['status'] == "success") {

        this.departmentList = datas['department'];
        this.designationList = datas['designation'];
        this.roleList = datas['role'];
        this.shiftData = datas['shift'];
      }

    });
  }

  // get designation
  //getDesignation() {
  //    $('.spinner').css('display', 'block');
  //    var requestObj = new RequestContainer();
  //    requestObj['employee_id'] = baseId;
  //    requestObj['accesskey'] = baseKey;
  //    requestObj['key_type'] = "designation";
  //    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetKeyValueData').subscribe(datas => {
  //        $('.spinner').css('display', 'none');
  //        this.designationList = datas['data'];
  //        this.getCompany();
  //    });

  //}

  getCompany() {
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetCompanyData').subscribe(datas => {
      $('.spinner').css('display', 'none');
      this.companyList = datas['data'];
      //this.getRole();
    });

  }

  //getRole() {
  //    $('.spinner').css('display', 'block');
  //    var requestObj = new RequestContainer();
  //    requestObj['employee_id'] = baseId;
  //    requestObj['accesskey'] = baseKey;
  //    requestObj['key_type'] = "role";
  //    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetKeyValueData').subscribe(datas => {
  //        $('.spinner').css('display', 'none');
  //        this.roleList = datas['data'];
  //    });

  //}

  // steps
  nextSptep1() {
    let status = this.validationHandler.validateDOM('stepid1');
    if (!status) {
      $(".step1").addClass('ng-hide');
      $(".step2").removeClass('ng-hide');
    }
  }
  previousSptep1() {
    $(".step2").addClass('ng-hide');
    $(".step1").removeClass('ng-hide');
  }
  nextSptep2() {
    let status = this.validationHandler.validateDOM('stepid2');
    if (!status) {
      $(".step2").addClass('ng-hide');
      $(".step3").removeClass('ng-hide');
    }
  }
  previousSptep2() {
    $(".step3").addClass('ng-hide');
    $(".step2").removeClass('ng-hide');
  }

  nextSptep3() {
    let status = this.validationHandler.validateDOM('stepid3');
    if (!status) {
      $(".step3").addClass('ng-hide');
      $(".step4").removeClass('ng-hide');
    }

  }
  previousSptep3() {
    $(".step4").addClass('ng-hide');
    $(".step3").removeClass('ng-hide');
  }
  nextSptep4() {
    let status = this.validationHandler.validateDOM('stepid4');
    if (!status) {
      $(".step4").addClass('ng-hide');
      $(".step5").removeClass('ng-hide');
    }
  }
  previousSptep4() {
    $(".step5").addClass('ng-hide');
    $(".step4").removeClass('ng-hide');
  }

  /*
  * create submit
  */
  createSubmit() {
    let status = this.validationHandler.validateDOM('profileupdateBasic');
    if (!status) {
      $('.spinner').css('display', 'block');
      this.submitObj['dob'] = $('#dob').datepicker().val();
      this.submitObj['doj'] = $('#doj').datepicker().val();
      this.submitObj['reporting_to'] = $("#reporting_to").val();
      this.submitObj['accesskey'] = baseKey;
      this.submitObj['employee_id'] = baseId;
      this.httpService.postRequest<ResponseContainer>(this.submitObj, baseUrl + 'api/AttendanceApi/CreateEmployee').subscribe(datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == 'success') {
          $(".step1").removeClass('ng-hide');
          $(".step2").addClass('ng-hide');
          $(".step3").addClass('ng-hide');
          $(".step4").addClass('ng-hide');
          $(".step5").addClass('ng-hide');
          this.submitObj = {};
          $("#changesubmit").modal('show');
        }
        else {

          this.errorObjData = datas['alert'];
          this.errorObjData1 = this.errorObjData['rsBody'];
          this.errorObjData2 = this.errorObjData1['exceptionBlock'];
          this.errorObjData2 = this.errorObjData2['msg'];
          this.validationHandler.displayErrors(this.errorObjData2['validationException'], "profileupdateBasic", null);
          $("#errorsubmit").modal('show');
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
      this.submitObj['reporting_to'] = $("#reporting_to").val();
      this.httpService.postRequest<ResponseContainer>(this.submitObj, baseUrl + 'api/AttendanceApi/UpdateEmployee').subscribe(datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == 'success') {
          $(".step1").removeClass('ng-hide');
          $(".step2").addClass('ng-hide');
          $(".step3").addClass('ng-hide');
          $(".step4").addClass('ng-hide');
          $(".step5").addClass('ng-hide');
          //this.submitObj = {};
          $("#changesubmit").modal('show');
        }
        else {

          this.errorObjData = datas['alert'];
          this.errorObjData1 = this.errorObjData['rsBody'];
          this.errorObjData2 = this.errorObjData1['exceptionBlock'];
          this.errorObjData2 = this.errorObjData2['msg'];
          this.validationHandler.displayErrors(this.errorObjData2['validationException'], "profileupdateBasic", null);
        }
      });
    }
  }

  // searh by name or id
  ngEmployeeTypeHead() {
    var payCode = this.submitObj['pay_code'];
    $("#reporting_to").autocomplete({
      source: function (request, response) {
        var credentials = { Prefix: request.term, employee_id: baseId, accesskey: baseKey, pay_code: payCode };
        $.ajax({

          url: baseUrl + "api/AttendanceApi/SearchEmployee",
          type: "POST",
          dataType: "json",
          data: credentials,
          success: function (datas) {
            this.typeHead = datas['data'];

            if (this.typeHead.length === 0) {
              label: "No Results."
            }
            else {
              //$scope.reData = $scope.table[0].Table;
              response($.map(this.typeHead, function (item) {
                return { label: item.name, value: item.employee_id };
              }))
            }
          }
        })
      },
      messages: {
        noResults: "", results: ""
      }
    });
  }


  onFocus() {
    this.validationHandler.hideErrors("profileupdateBasic");
    this.validationHandler.hideErrors("stepid1");
    this.validationHandler.hideErrors("stepid2");
    this.validationHandler.hideErrors("stepid3");
    this.validationHandler.hideErrors("stepid4");
  }
}
