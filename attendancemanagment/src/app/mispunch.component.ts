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
  templateUrl: './pages/mispunch.component.html',
  providers: [HttpService, ValidationHandler],
  //styleUrls: [_cssURL]
})
export class MispunchComponent implements OnInit {
  public responseData: any = {};
  public hrs: any = [];
  public mint: any = [];
  public submitObj: any = {};
  public sendQuery: any = {};
  public setCreate: boolean = true;
  public targetindex: any = {};
  public shiftData: any = [];
  public SelectType: any = {};
  @Input() elementId: String;
  @Output() onEditorKeyup = new EventEmitter<any>();
  editor;


  /*
  * get employee
  */
  ngOnInit() {

    $("#mispunchdate").datepicker(
      {
        dateFormat: 'dd/mm/yy',
      });

    $('.spinner').css('display', 'block');
    var requestObj = new RequestContainer();
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetHrsMint').subscribe(
      datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == "success") {
          this.responseData = datas['role']
          this.hrs = datas['hrs'];
          this.mint = datas['mint'];
        }
      });

    this.getShift();
  }

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
  * send query
  */
  /*
* create submit
*/
  createSubmit() {
    let status = this.validationHandler.validateDOM('profileupdateBasic');
    if (!status) {
      $('.spinner').css('display', 'block');
      this.sendQuery['employee_id'] = baseId;
      this.sendQuery['accesskey'] = baseKey;
      this.sendQuery['p_date'] = $('#mispunchdate').datepicker().val();
      this.sendQuery['employee_code'] = $('#employee_code').val();
      this.sendQuery['type'] = "attRequest";
      this.sendQuery['in_time'] = this.sendQuery['in_hourse'] + ':' + this.sendQuery['in_minute'];
      this.sendQuery['out_time'] = this.sendQuery['out_hourse'] + ':' + this.sendQuery['out_minute'];
      this.httpService.postRequest<ResponseContainer>(this.sendQuery, baseUrl + 'api/AttendanceApi/MispunchRequest').subscribe(datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == "success") {
          $("#changesubmit").modal('show');
          this.sendQuery = {};
          this.submitObj = {};
          this.ngOnInit();
        }
      });
    }
  }


  onFocus() {
    this.validationHandler.hideErrors("profileupdateBasic");
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


}
