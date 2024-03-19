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
  templateUrl: './pages/daily-attendance.component.html',
  providers: [HttpService, ValidationHandler],
  //styleUrls: [_cssURL]
})
export class DailyAttendanceComponent implements OnInit {
  public responseData: any = {};
  public baseUrlLink: any = {};
  public viewList: any = [];
  public departmentList: any = [];
  public shiftData: any = [];
  public submitObj: any = {};
  public pageNo: Number = 0;
  public defaultNo: Number = 0;
  public pageSize: Number = 100;
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
      changeMonth: true
    }).datepicker("setDate", new Date());

    $('#to_date').datepicker({
      dateFormat: 'dd/mm/yy',
      changeMonth: true
    }).datepicker("setDate", new Date());

    this.baseUrlLink = baseUrl;

    $('.spinner').css('display', 'block');
    this.submitObj['from_date'] = $('#from_date').datepicker().val();
    this.submitObj['to_date'] = $('#to_date').datepicker().val();
    this.submitObj['accesskey'] = baseKey;
    this.submitObj['employee_id'] = baseId;
    this.submitObj['pageNo'] = 0;
    this.submitObj['pageSize'] = this.pageSize;
    this.pageNo = 0;
    this.httpService.postRequest<ResponseContainer>(this.submitObj, baseUrl + 'api/AttendanceApi/GetDailyAttendance').subscribe(datas => {

      if (datas['status'] == 'success') {
        this.viewList = datas['data'];
        //this.viewList = this.responseData['attendaceList'];
        if (this.viewList != null) {
          this.searchEmpObj = true;
          if (this.viewList.length == this.pageSize) {
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
        }
        else {
          this.viewList = [];
        }

        //$('.spinner').css('display', 'none');

      }
      this.getDepartment();
    });

    //this.submitObj['from_date'] = new Date();



  }

  // department
  getDepartment() {
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    requestObj['key_type'] = "department";
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetKeyValueData').subscribe(datas => {
      $('.spinner').css('display', 'none');
      if (datas['status'] == "success") {
        this.departmentList = datas['data'];
        this.getShift();
      }

    });

  }

  // get shift
  getShift() {
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetCompanyData').subscribe(datas => {
      $('.spinner').css('display', 'none');
      this.shiftData = datas['data'];
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

  // get data base on shift
  getByShift() {
    this.submitObj['employee_code'] = "";
    this.submitObj['department'] = "";
    this.submitObj['status'] = "";
    this.searchSubmit();
  }
  getByStatus() {
    this.submitObj['employee_code'] = "";
    this.submitObj['department'] = "";
    this.submitObj['shift'] = "";
    this.searchSubmit();
  }
  getByDepartment() {
    this.submitObj['employee_code'] = "";
    this.submitObj['status'] = "";
    this.submitObj['shift'] = "";
    this.searchSubmit();
  }
  searchSubmit() {
    let status = this.validationHandler.validateDOM('profileupdateBasic');
    if (!status) {
      $('.spinner').css('display', 'block');
      this.submitObj['from_date'] = $('#from_date').datepicker().val();
      this.submitObj['to_date'] = $('#to_date').datepicker().val();
      this.submitObj['accesskey'] = baseKey;
      this.submitObj['employee_id'] = baseId;
      this.submitObj['pageNo'] = 0;
      this.submitObj['pageSize'] = this.pageSize;
      this.pageNo = 0;
      this.httpService.postRequest<ResponseContainer>(this.submitObj, baseUrl + 'api/AttendanceApi/GetDailyAttendance').subscribe(datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == 'success') {
          this.viewList = datas['data'];
          //this.viewList = this.responseData['attendaceList'];
          if (this.viewList != null) {
            this.searchEmpObj = true;
            if (this.viewList.length == this.pageSize) {
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
          }
          else {
            this.viewList = [];
          }

        }
      });
    }
  }

  /*
* pagination next
*/
  nextSubmit() {

    $('.spinner').css('display', 'block');
    this.submitObj['from_date'] = $('#from_date').datepicker().val();
    this.submitObj['to_date'] = $('#to_date').datepicker().val();
    this.submitObj['accesskey'] = baseKey;
    this.submitObj['employee_id'] = baseId;
    this.submitObj['pageNo'] = Number(this.pageNo) + Number(1);
    this.submitObj['pageSize'] = this.pageSize;
    this.pageNo = this.submitObj['pageNo'];

    this.httpService.postRequest<ResponseContainer>(this.submitObj, baseUrl + 'api/AttendanceApi/GetDailyAttendance').subscribe(
      datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == 'success') {
          this.viewList = datas['data'];
          //this.viewList = this.responseData['attendaceList'];
          if (this.viewList != null) {
            this.searchEmpObj = true;
            if (this.viewList.length == this.pageSize) {
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
          }
          else {
            this.viewList = [];
          }

        }
      });

  }

  /*
  * pagination previous
  */
  previousSubmit() {

    if (this.pageNo == 0) {
      this.submitObj['pageNo'] = 0;
      this.pageNo = this.submitObj['pageNo'];
    }
    else {
      this.submitObj['pageNo'] = Number(this.pageNo) - 1;
      this.pageNo = this.submitObj['pageNo'];
    }

    $('.spinner').css('display', 'block');
    this.submitObj['from_date'] = $('#from_date').datepicker().val();
    this.submitObj['to_date'] = $('#to_date').datepicker().val();
    this.submitObj['accesskey'] = baseKey;
    this.submitObj['employee_id'] = baseId;
    this.submitObj['pageSize'] = this.pageSize;

    this.httpService.postRequest<ResponseContainer>(this.submitObj, baseUrl + 'api/AttendanceApi/GetDailyAttendance').subscribe(
      datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == 'success') {
          this.viewList = datas['data'];
          //this.viewList = this.responseData['attendaceList'];
          if (this.viewList != null) {
            this.searchEmpObj = true;
            if (this.viewList.length == this.pageSize) {
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
          }
          else {
            this.viewList = [];
          }

        }
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

  pdfSubmit() {

    //var GridHtml = $("#Gridpdf").html();
    ////$("input[name='GridHtml']").val($("#Grid").html());
    //$('#ExportExcelData').attr('href', '../Administrator/Export?name=' + GridHtml);
    //$('#ExportExcelData')[0].click();
    // var GridHtml = $("#Gridpdf").html();
    //$.ajax({
    //    url: "http://localhost:25182/Administrator/Export",
    //    type: "POST",
    //    dataType: "json",
    //    data: { GridHtml: GridHtml },
    //    success: function (datas) {
    //        console.log(datas);
    //    }
    //});

    //var GridHtml = $("#Gridpdf").html();
    ////$("input[name='GridHtml']").val($("#Grid").html());
    //$('#ExportExcelData').attr('href', '../Administrator/ExportPDF?name=' + GridHtml);
    //$('#ExportExcelData')[0].click();

    $('.spinner').css('display', 'block');
    this.httpService.postRequest<ResponseContainer>(this.submitObj, baseUrl + 'api/AttendanceApi/GenerateDailyAttendancePDF').subscribe(datas => {
      $('.spinner').css('display', 'none');
      if (datas['status'] == "success") {

        $('#ExportExcelData').attr('href', '../Images/' + datas['data']);
        $('#ExportExcelData')[0].click();

      }
    });

  }


  onFocus() {
    this.validationHandler.hideErrors("profile-submit");
  }


}
