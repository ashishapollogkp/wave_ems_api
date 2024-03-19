import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { HttpService } from '../common/service/http.service';
import { HttpRequestObj } from '../common/service/HttpRequestObj';
import { ValidationHandler } from '../common/ValidationHandler';
import { ResponseContainer } from '../common/ResponseContainer';
import { RequestContainer } from '../common/RequestContainer';

//import { environment } from '@environments/environment';
import { User } from './user';

declare var $: any;

var baseUrl = $('#BaseUrl').data('baseurl');
var baseId = $('#sidudethmc').data('bind');
var baseKey = $('#activKey').data('bind');

@Injectable({ providedIn: 'root' })
export class AuthenticationService {
    private currentUserSubject: BehaviorSubject<User>;
    public currentUser: Observable<User>;
    public sdObj: Object = {};

    constructor(private http: HttpClient, private router: Router,) {
        this.currentUserSubject = new BehaviorSubject<User>(JSON.parse(localStorage.getItem('currentUser')));
        this.currentUser = this.currentUserSubject.asObservable();
    }

    public get currentUserValue(): User {
        return this.currentUserSubject.value;
    }

    login() {

        this.sdObj['employee_id'] = baseId;
        this.sdObj['accesskey'] = baseKey;
        this.sdObj['currentUrl'] = this.router.url;
        console.log(this.router.url);
        return this.http.post<any>(baseUrl + 'api/AttendanceApi/UrlAuthontication', this.sdObj)
            .pipe(map(datas => {
                // login successful if there's a jwt token in the response

                if (datas['status'] != 'success') {
                    localStorage.setItem('currentUser', datas);
                    this.currentUserSubject.next(datas);
                }

                return datas;
            }));
        //this.httpService.postRequest<ResponseContainer>(this.sdObj, baseUrl + 'api/AttendanceApi/UrlAuthontication').subscribe(datas => {
        //       $('.spinner').css('display', 'none');
        //         ///return datas;
        //        //this.shiftData = datas['data'];
        //    });

        //return User;
    }

    logout() {
        // remove user from local storage to log user out
        localStorage.removeItem('currentUser');
        this.currentUserSubject.next(null);
    }
}