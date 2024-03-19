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
  templateUrl: './pages/formtwelveforteen.component.html',
  providers: [HttpService, ValidationHandler],
  //styleUrls: [_cssURL]
})
export class Form12Component implements OnInit {

  public responseData: any = {};
  public viewList: any = [];
  public submitObj: any = {};
  public pageNo: Number = 0;
  public defaultNo: Number = 0;
  public pageSize: any = {};
  public searchEmpObj: any = false;
  public setPageNo: any = false;
  public setPageNoNext: any = false;
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
    let status = this.validationHandler.validateDOM('profileupdateBasic');
    if (!status) {
      $('.spinner').css('display', 'block');
      this.submitObj['from_date'] = $('#from_date').datepicker().val();
      this.submitObj['to_date'] = $('#to_date').datepicker().val();
      this.submitObj['accesskey'] = baseKey;
      this.submitObj['employee_id'] = baseId;

      $('.spinner').css('display', 'block');
      this.httpService.postRequest<ResponseContainer>(this.submitObj, baseUrl + 'api/AttendanceApi/GenerateFormPDF').subscribe(datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == "success") {

          $('#ExportExcelData').attr('href', '../Images/' + datas['data']);
          $('#ExportExcelData')[0].click();

        }
      });
    }
  }

  onFocus() {
    this.validationHandler.hideErrors("profile-submit");
  }


}
