import { Injectable } from '@angular/core';
import { Router, CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Location } from "@angular/common";
import { HttpService } from '../common/service/http.service';
import { HttpRequestObj } from '../common/service/HttpRequestObj';
import { ValidationHandler } from '../common/ValidationHandler';
import { ResponseContainer } from '../common/ResponseContainer';
import { RequestContainer } from '../common/RequestContainer';
import { AuthenticationService } from './authentication.service';
declare var $: any;

var baseUrl = $('#BaseUrl').data('baseurl');
var baseId = $('#sidudethmc').data('bind');
var baseKey = $('#activKey').data('bind');

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
    public currentUser: Object = {};
    constructor(
        private router: Router,
        private httpService: HttpService,
        private authenticationService: AuthenticationService,
        public location: Location
    ) { }

    //canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
    //    //const currentUser = this.authenticationService.login;
    //    this.currentUser = this.authenticationService.login;
    //    if (this.currentUser != null) {
    //        // check if route is restricted by role
    //        //if (route.data.roles && route.data.roles.indexOf(currentUser.status) === -1) {
    //        //    // role not authorised so redirect to home page
    //        //    this.router.navigate(['']);
    //        //    return false;
    //        //}

    //        // authorised so return true
    //        return true;
    //    }

    //    // not logged in so redirect to login page with the return url
    //    this.router.navigate([''], { queryParams: { returnUrl: state.url } });
    //    return false;
    //}

    canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
        //const currentUser = this.authenticationService.currentUserValue;
        if (baseId != null) {

            setTimeout(() => {
                var requestObj = new RequestContainer();
                requestObj['employee_id'] = baseId;
                requestObj['accesskey'] = baseKey;
                console.log(this.router.url);
                requestObj['currentUrl'] = this.router.url;
                this.httpService.postRequest<ResponseContainer>(requestObj, baseUrl + 'api/AttendanceApi/UrlAuthontication').subscribe(datas => {
                    $('.spinner').css('display', 'none');
                    if (datas['status'] == "success") {
                        return true;
                        this.router.navigate([this.router.url]);
                        //this.router.navigate([this.router.url], { queryParams: { returnUrl: state.url } });
                    }
                    else {
                        //this.router.navigate([''], { queryParams: { returnUrl: state.url } });
                        this.router.navigate(['']);
                        return false;
                    }
                    //this.shiftData = datas['data'];
                });
            }, 1000);
            
            
        }
       return true;
    }
}