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
  templateUrl: './pages/offerletter.component.html',
  providers: [HttpService, ValidationHandler],
  //styleUrls: [_cssURL]
})
export class OfferLetterComponent implements OnInit {
  public responseData: any = [];
  public tabChange: any = {};
  public errorObjData: any = {};
  public errorObjData1: any = {};
  public errorObjData2: any = {};
  public submitObj: any = {};
  public designationList: any = [];
  public departmentList: any = [];
  public companyList: any = [];
  public setCreate: Boolean = true;
  public setUpdate: Boolean = false;
  public pageNo: Number = 0;
  public defaultNo: Number = 0;
  public pageSize: any = {};
  public setPageNo: boolean = false;
  public setPageNoNext: boolean = false;
  @Input() elementId: String;
  @Output() onEditorKeyup = new EventEmitter<any>();
  editor;


  /*
  * get offer letter
  */
  ngOnInit() {

    $("#offer_date").datepicker({ dateFormat: 'dd/mm/yy' }).datepicker("setDate", new Date());
    this.tabChange = "viewLeave";

    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    this.pageSize = 10;
    requestObj['pageNo'] = 0;
    requestObj['pageSize'] = this.pageSize;
    this.pageNo = requestObj['pageNo'];
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetOfferLetter').subscribe(
      datas => {
        $('.spinner').css('display', 'none');
        this.responseData = datas['data'];

        if (datas['status'] == "success") {
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
        }
        else {
          this.responseData = [];
        }


        this.getDepartment();
      });
  }
  //ngOnInit() {


  //    $("#offer_date").datepicker({ dateFormat: 'dd/mm/yy'}).datepicker("setDate", new Date());
  //    $('.spinner').css('display', 'block');
  //    this.tabChange = "viewLeave";
  //    var requestObj = new RequestContainer();
  //    requestObj['employee_id'] = baseId;
  //    requestObj['accesskey'] = baseKey;
  //    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetOfferLetter').subscribe(
  //        datas => {
  //            $('.spinner').css('display', 'none');
  //            if (datas['status'] == "success") {
  //                this.responseData = datas['data'];
  //            }

  //            this.getDepartment();        
  //        });
  //}

  // routing
  constructor(

    private router: Router,
    private sanitization: DomSanitizer,
    private httpService: HttpService,
    private validationHandler: ValidationHandler
  ) {

  }

  // get depatment
  getDepartment() {
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    requestObj['key_type'] = "department";
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetKeyValueData').subscribe(datas => {
      $('.spinner').css('display', 'none');
      this.departmentList = datas['data'];
      this.getDesignation();
    });

  }

  // get designation
  getDesignation() {
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    requestObj['key_type'] = "designation";
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetKeyValueData').subscribe(datas => {
      $('.spinner').css('display', 'none');
      this.designationList = datas['data'];
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

  /*
  * create submit
  */
  createSubmit() {
    let status = this.validationHandler.validateDOM('profileupdateBasic');
    if (!status) {
      $('.spinner').css('display', 'block');
      this.submitObj['offer_date'] = $('#offer_date').datepicker().val();
      this.submitObj['reporting_to'] = $("#reporting_to").val();
      this.submitObj['accesskey'] = baseKey;
      this.submitObj['employee_id'] = baseId;
      this.httpService.postRequest<ResponseContainer>(this.submitObj, baseUrl + 'api/AttendanceApi/CreateOfferLetter').subscribe(datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == 'success') {
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

  // download offerletter
  downloadOfferLetter(Obj) {
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['employee_id'] = baseId;
    requestObj['id'] = Obj;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/DownloadOfferLetter').subscribe(datas => {
      $('.spinner').css('display', 'none');
      if (datas['status'] == 'success') {
        $('#ExportExcelData').attr('href', '../Images/' + datas['data']);
        $('#ExportExcelData')[0].click();
      }
    });
  }

  // change tab

  changeTabSubmit(obj) {

    $('.active').removeClass('active');
    $('.' + obj).addClass('active');
    if (obj == "viewMispunch") {
      $(".teamattendace").css("display", "block");
    }
    else {
      $(".teamattendace").css("display", "none");
    }
    this.tabChange = obj;
  }

  // searh by name or id
  ngEmployeeTypeHead() {
    $("#reporting_to").autocomplete({
      source: function (request, response) {
        var credentials = { Prefix: request.term, employee_id: baseId, accesskey: baseKey };
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
  }

}
