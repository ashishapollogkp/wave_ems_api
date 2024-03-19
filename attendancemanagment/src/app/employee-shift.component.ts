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
  templateUrl: './pages/employee-shift.component.html',
  providers: [HttpService, ValidationHandler],
  //styleUrls: [_cssURL]
})
export class EmployeeShiftComponent implements OnInit {
  public responseData: any = [];
  public viewList: any = [];
  public departmentList: any = [];
  public shiftData: any = [];
  public submitObj: any = {};
  public SelectType: any = {};
  public tabChange: any = {};
  public pageNo: Number = 0;
  public defaultNo: Number = 0;
  public pageSize: Number = 100;
  public searchEmpObj: boolean = false;
  public setPageNo: boolean = false;
  public setPageNoNext: boolean = false;
  public setCreate: Boolean = true;
  public setUpdate: Boolean = false;
  public targetindex: any = {};
  public errorObjData: any = {};
  @Input() elementId: String;
  @Output() onEditorKeyup = new EventEmitter<any>();
  editor;


  /*
  * get employee
  */
  ngOnInit() {
    $('#from_date').datepicker({
      dateFormat: 'dd/mm/yy'
    });

    $('#to_date').datepicker({
      dateFormat: 'dd/mm/yy'
    });

    $('#from_date1').datepicker({
      dateFormat: 'dd/mm/yy'
    });

    $('#to_date1').datepicker({
      dateFormat: 'dd/mm/yy'
    });

    $('.spinner').css('display', 'block');
    //this.submitObj['from_date'] = $('#from_date').datepicker().val();
    //this.submitObj['to_date'] = $('#to_date').datepicker().val();
    this.tabChange = "view_id";
    this.submitObj['accesskey'] = baseKey;
    this.submitObj['employee_id'] = baseId;
    this.submitObj['pageNo'] = 0;
    this.submitObj['pageSize'] = this.pageSize;
    this.pageNo = 0;
    this.httpService.postRequest<ResponseContainer>(this.submitObj, baseUrl + 'api/AttendanceApi/GetEmployeeShiftData').subscribe(datas => {
      $('.spinner').css('display', 'none');
      if (datas['status'] == 'success') {
        this.responseData = datas['data'];
        //this.viewList = this.responseData['attendaceList'];
        if (this.responseData != null) {
          this.searchEmpObj = true;
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
        }
        else {
          this.responseData = [];
        }

      }
      this.getShift();
    });

  }

  // get shift
  getShift() {
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetShift').subscribe(datas => {
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
  searchSubmit() {
    let status = this.validationHandler.validateDOM('profileupdateBasic2');
    if (!status) {
      $('.spinner').css('display', 'block');
      var requestObj = new RequestContainer();
      requestObj['employee_id'] = baseId;
      requestObj['accesskey'] = baseKey;
      requestObj['from_date'] = $('#from_date1').datepicker().val();
      requestObj['to_date'] = $('#to_date1').datepicker().val();
      requestObj['employee_code'] = this.submitObj['employee_code1'];
      requestObj['shift'] = this.submitObj['shift1'];
      requestObj['accesskey'] = baseKey;
      requestObj['employee_id'] = baseId;
      requestObj['pageNo'] = 0;
      requestObj['pageSize'] = this.pageSize;
      this.pageNo = 0;
      this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetEmployeeShiftData').subscribe(datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == 'success') {
          this.responseData = datas['data'];
          //this.viewList = this.responseData['attendaceList'];
          if (this.responseData != null) {
            this.searchEmpObj = true;
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
    var requestObj = new RequestContainer();
    requestObj['from_date'] = $('#from_date1').datepicker().val();
    requestObj['to_date'] = $('#to_date1').datepicker().val();
    requestObj['employee_code'] = this.submitObj['employee_code1'];
    requestObj['shift'] = this.submitObj['shift1'];
    requestObj['accesskey'] = baseKey;
    requestObj['employee_id'] = baseId;
    requestObj['pageNo'] = Number(this.pageNo) + Number(1);
    requestObj['pageSize'] = this.pageSize;
    this.pageNo = requestObj['pageNo'];

    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetEmployeeShiftData').subscribe(
      datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == 'success') {
          this.responseData = datas['data'];
          //this.viewList = this.responseData['attendaceList'];
          if (this.responseData != null) {
            this.searchEmpObj = true;
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

    var requestObj = new RequestContainer();
    if (this.pageNo == 0) {
      requestObj['pageNo'] = 0;
      this.pageNo = requestObj['pageNo'];
    }
    else {
      requestObj['pageNo'] = Number(this.pageNo) - 1;
      this.pageNo = requestObj['pageNo'];
    }

    $('.spinner').css('display', 'block');
    requestObj['from_date'] = $('#from_date1').datepicker().val();
    requestObj['to_date'] = $('#to_date1').datepicker().val();
    requestObj['employee_code'] = this.submitObj['employee_code1'];
    requestObj['shift'] = this.submitObj['shift1'];
    requestObj['accesskey'] = baseKey;
    requestObj['employee_id'] = baseId;
    requestObj['pageSize'] = this.pageSize;

    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetEmployeeShiftData').subscribe(
      datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == 'success') {
          this.responseData = datas['data'];
          // this.responseData = this.responseData['attendaceList'];
          if (this.responseData != null) {
            this.searchEmpObj = true;
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
          }
          else {
            this.viewList = [];
          }

        }
      });

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
      this.submitObj['employee_code'] = $('#employee_code').val();
      this.submitObj['from_date'] = $('#from_date').datepicker().val();
      this.submitObj['to_date'] = $('#to_date').datepicker().val();
      this.httpService.postRequest<ResponseContainer>(this.submitObj, baseUrl + 'api/AttendanceApi/CreateEmployeeShift').subscribe(datas => {
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
      this.submitObj['from_date'] = $('#from_date').datepicker().val();
      this.submitObj['employee_code'] = $('#employee_code').val();
      this.submitObj['to_date'] = $('#to_date').datepicker().val();
      this.httpService.postRequest<ResponseContainer>(this.submitObj, baseUrl + 'api/AttendanceApi/UpdateEmployeeShift').subscribe(datas => {
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
    this.httpService.postRequest<ResponseContainer>(this.submitObj, baseUrl + 'api/AttendanceApi/GeneratePDF').subscribe(datas => {
      $('.spinner').css('display', 'none');
      if (datas['status'] == "success") {

        $('#ExportExcelData').attr('href', '../Images/' + datas['data']);
        $('#ExportExcelData')[0].click();

      }
    });

  }

  // search employee 
  typeHeadSubmit() {

    var loginType = this.SelectType;
    $("#employee_code").autocomplete({
      source: function (request, response) {
        $.ajax({
          url: baseUrl + "/Administrator/EmployeeTypehead",
          type: "POST",
          dataType: "json",
          data: { Prefix: request.term, employee_id: baseId, accessKey: baseKey },
          success: function (datas) {
            response($.map(datas, function (item) {
              return { label: item.name, value: item.employee_id };
            }))

          }
        })
      },
      messages: {
        noResults: "", results: ""
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

  onFocus1() {
    this.validationHandler.hideErrors("profileupdateBasic2");
  }


}
