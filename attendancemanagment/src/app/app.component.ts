import { Component } from '@angular/core';
import {HttpClient, HttpHeaders, HttpEvent, HttpResponse} from '@angular/common/http';
import { Router, ActivatedRoute, Params} from '@angular/router';
import {by, element} from 'protractor';
import { DomSanitizer, SafeHtml, SafeUrl, SafeStyle } from '@angular/platform-browser';
import { HttpService } from '../common/service/http.service';
import { HttpRequestObj } from '../common/service/HttpRequestObj';
import { ValidationHandler } from '../common/ValidationHandler';
import { ResponseContainer } from '../common/ResponseContainer';
import { BaseContainer } from '../common/BaseContainer';
import { RequestContainer } from '../common/RequestContainer';

declare var $: any;

var baseUrl = $('#BaseUrl').data('baseurl');
var baseId = $('#sidudethmc').data('bind');
var baseKey = $('#activKey').data('bind');

//var _templateURL = baseUrl + 'Administrator/App';

//var _templateURL = './app.component.html';

@Component({
    selector: 'app-root',
    templateUrl: './pages/app.component.html',
    providers: [HttpService],
    //styleUrls: [_cssURL]
})


export class AppComponent {

    public employeeDetail: any = {};
    public responseData: any = {};
    public location: any = {};
    returnUrl: string;

    private httpOptions = {
        headers: new HttpHeaders({ 'Content-Type': 'application/json' })
    };

    constructor(
        private router: Router,
        private route: ActivatedRoute,
        private httpService: HttpService,
    ) {

    }

    // Get Current User Login Details
    ngOnInit() {
        $('.spinner').css('display', 'block');
        var requestObj = new RequestContainer();
        requestObj['employee_id'] = baseId;
        requestObj['accesskey'] = baseKey;
        this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/EmployeeDetails').subscribe(
            responseObj => {
                $('.spinner').css('display', 'none');
                if (responseObj['status'] == 'success') {
                    this.employeeDetail = responseObj['data'];
                }
                //this.responseData = responseObj;
               // this.employeeDetail = this.responseData['data'];
            });
    }
  //title = 'Resources';
}
