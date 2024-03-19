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
  templateUrl: './pages/payslip.component.html',
  providers: [HttpService, ValidationHandler],
  //styleUrls: [_cssURL]
})
export class PayslipComponent implements OnInit {
  public responseData: any = [];
  public targetindex: any = {};
  public errorObjData: any = {};
  public keyValueList: any = [];
  public categories: any = {};
  public submitObj: any = {};
  public resData: any = {};
  public pageNo: Number = 0;
  public defaultNo: Number = 0;
  public pageSize: any = {};
  public setCreate: any = true;
  public setUpdate: any = false;
  public setPageNo: any = false;
  public setPageNoNext: any = false;
  @Input() elementId: String;
  @Output() onEditorKeyup = new EventEmitter<any>();
  editor;


  ngOnInit() {

    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    this.pageSize = 10;
    requestObj['pageNo'] = 0;
    requestObj['pageSize'] = this.pageSize;
    this.pageNo = requestObj['pageNo'];
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetSalarySlip').subscribe(
      responseObj => {
        $('.spinner').css('display', 'none');
        if (responseObj['status'] == "success") {
          this.responseData = responseObj['data'];
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
          this.responseData = []
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

  // download salary slip
  downloadPayslipSubmit(Obj) {
    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['employee_id'] = baseId;
    requestObj['id'] = Obj;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/DownloadPayslip').subscribe(datas => {
      $('.spinner').css('display', 'none');
      if (datas['status'] == 'success') {
        $('#ExportExcelData').attr('href', '../Images/' + datas['data']);
        $('#ExportExcelData')[0].click();
      }
    });
  }

  onFocus() {
    this.validationHandler.hideErrors("profileupdateBasic");
  }


}
