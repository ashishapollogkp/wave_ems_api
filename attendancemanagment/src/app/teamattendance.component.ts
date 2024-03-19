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
  templateUrl: './pages/teamattendance.component.html',
  providers: [HttpService, ValidationHandler],
  //styleUrls: [_cssURL]
})
export class TeamAttendanceComponent implements OnInit {
  public responseData: any = [];
  public tabChange: any = {};
  @Input() elementId: String;
  @Output() onEditorKeyup = new EventEmitter<any>();
  editor;


  /*
  * get employee
  */
  ngOnInit() {


    $("#attendaceDate").datepicker({ dateFormat: 'dd/mm/yy' }).datepicker("setDate", new Date());
    $('.spinner').css('display', 'block');
    this.tabChange = "viewLeave";
    var requestObj = new RequestContainer();
    requestObj['employee_id'] = baseId;
    requestObj['accesskey'] = baseKey;
    this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetTeamAttendance').subscribe(
      datas => {
        $('.spinner').css('display', 'none');
        if (datas['status'] == "success") {
          this.responseData = datas['data'];
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
  * search employee
  */
  ApplyFilter() {
    let status = this.validationHandler.validateDOM('filtersubmit');
    if (!status) {
      $('.spinner').css('display', 'block');
      var requestObj = new RequestContainer();
      requestObj['employee_id'] = baseId;
      requestObj['accesskey'] = baseKey;
      requestObj['date'] = $('#attendaceDate').datepicker().val();
      this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/GetTeamAttendance').subscribe(
        datas => {
          $('.spinner').css('display', 'none');
          if (datas['status'] == "success") {
            this.responseData = datas['data'];
          }
        });
    }
  }

  // employee attendance details
  teamSubmit(team_id) {
    this.router.navigate(['/my-team', team_id]);
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

  onFocus() {
    this.validationHandler.hideErrors("filtersubmit");
  }

}
