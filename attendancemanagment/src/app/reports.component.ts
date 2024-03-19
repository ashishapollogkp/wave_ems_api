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
  templateUrl: './pages/reports.component.html',
  providers: [HttpService, ValidationHandler],
  //styleUrls: [_cssURL]
})
export class ReportsComponent implements OnInit {

  public responseData: any = {};
  public baseUrlLink: any = {};
  public viewList: any = [];
  public departmentList: any = [];
  public shiftData: any = [];
  public submitObj: any = {};
  public pageNo: Number = 0;
  public defaultNo: Number = 0;
  public pageSize: any = {};
  public searchEmpObj: boolean = false;
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
    $('#from_date').datepicker({
      dateFormat: 'dd/mm/yy',
    });

    $('#to_date').datepicker({
      dateFormat: 'dd/mm/yy',
    });

    this.baseUrlLink = baseUrl;

    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetCompanyData').subscribe(datas => {
      $('.spinner').css('display', 'none');
      this.shiftData = datas['data'];
    });

    //$('.spinner').css('display', 'block');
    //var requestObj = new RequestContainer();
    //requestObj['employee_id'] = baseId;
    //requestObj['accesskey'] = baseKey;
    //requestObj['key_type'] = "department";
    //this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetKeyValueData').subscribe(datas => {
    //    $('.spinner').css('display', 'none');
    //    if (datas['status'] == "success") {
    //        this.departmentList = datas['data'];
    //        //this.getShift();
    //    }

    //});
  }

  // get shift
  onChangeGetDepartment() {
    this.responseData = [];
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    requestObj['key_type'] = "department";
    requestObj['pay_code'] = this.submitObj['pay_code'];
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetDepartmentData').subscribe(datas => {
      $('.spinner').css('display', 'none');
      this.departmentList = datas['data'];
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
  //searchSubmit()
  //{
  //    let status = this.validationHandler.validateDOM('profileupdateBasic');
  //    if (!status) {
  //        this.submitObj['from_date'] = $('#from_date').datepicker().val();
  //        this.submitObj['to_date'] = $('#to_date').datepicker().val();
  //        $('#ExportExcelData').attr('href', '../Administrator/ExportExcel?name=' + this.submitObj['type'] + '&fromDate=' + this.submitObj['from_date'] + '&toDate=' + this.submitObj['to_date']);
  //        $('#ExportExcelData')[0].click();
  //        //$('.spinner').css('display', 'block');
  //        //this.submitObj['from_date'] = $('#from_date').datepicker().val();
  //        //this.submitObj['to_date'] = $('#to_date').datepicker().val();
  //        //this.submitObj['accesskey'] = baseKey;
  //        //this.submitObj['employee_id'] = baseId;
  //        //this.httpService.postRequest<ResponseContainer>(this.submitObj, baseUrl + 'api/AttendanceApi/DownloadReports').subscribe(datas => {
  //        //    $('.spinner').css('display', 'none');
  //        //    if (datas['status'] == 'success') {
  //        //        this.responseData = datas['data'];
  //        //        this.viewList = this.responseData['attendaceList'];
  //        //        this.searchEmpObj = true;
  //        //    }
  //        //});
  //    }
  //  }

  searchSubmit() {
    let status = this.validationHandler.validateDOM('profileupdateBasic');
    if (!status) {
      $('.spinner').css('display', 'block');
      this.submitObj['from_date'] = $('#from_date').datepicker().val();
      this.submitObj['to_date'] = $('#to_date').datepicker().val();
      this.submitObj['accesskey'] = baseKey;
      this.submitObj['employee_id'] = baseId;
      this.pageNo = 0;
      this.httpService.postRequest<ResponseContainer>(this.submitObj, baseUrl + 'api/AttendanceApi/GetAttendanceReport').subscribe(datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == 'success') {
          this.responseData = datas['data'];
          //this.viewList = this.responseData['attendaceList'];
          if (this.responseData != null) {
            this.searchEmpObj = true;
          }
          else {
            this.responseData = [];
          }

        }
      });
    }
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

  pdfSubmit() {

    $('.spinner').css('display', 'block');
    this.httpService.postRequest<ResponseContainer>(this.submitObj, baseUrl + 'api/AttendanceApi/GenerateReportPDF').subscribe(datas => {
      $('.spinner').css('display', 'none');
      if (datas['status'] == "success") {

        $('#ExportExcelData').attr('href', '../Images/' + datas['data']);
        $('#ExportExcelData')[0].click();

      }
    });

  }


  onFocus() {
    this.validationHandler.hideErrors("profileupdateBasic");
  }


}
