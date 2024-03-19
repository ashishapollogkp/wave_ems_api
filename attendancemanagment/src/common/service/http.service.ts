import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders,HttpEvent,HttpResponse} from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import { of } from 'rxjs/observable/of';
import { catchError, map, tap } from 'rxjs/operators';

import {BaseContainer} from '../BaseContainer';
import {HttpRequestObj} from './HttpRequestObj';
import {HttpResponseObj} from './HttpResponseObj';

declare var $ :any;

@Injectable()
export class HttpService {

    private  httpOptions = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json'})
    };


    //'Authorization': $('input[name=__RequestVerificationToken]').val() 

 httpEvent: HttpResponse<HttpResponseObj>;

 public trackObj: any = {};
 public location:any = {};
 public ipInfoToken:any = '7c8b8f2eab6adf';

 public baseurlpath = "http://localhost:44178";

  constructor(private http: HttpClient) {

    //get the user ip details using a jsonp response

  }

  //GENRIC POST request
  public postRequest<T extends BaseContainer>(requestObj  , url)
  :Observable<T>
  {
      var parentRef = this;
    //var httpRequestObj = new HttpRequestObj(requestObj);

    //create new observable of the return type which will be executed as completed
    //as and when we get back the response from http

     // var headers = new Headers({ 'Authorization': $('input[name=__RequestVerificationToken]').val() });
      var currentUser = $('input[name=__RequestVerificationToken]').val();
      //var headers_object = new HttpHeaders();
      //headers_object.append('Content-Type', 'application/json');
      //headers_object.append("Authorization", "Bearer " + currentUser);

      //var headers_object = new HttpHeaders().set("Content-Type", "application/json ");
      var headers_object = new HttpHeaders().set("Authorization", currentUser);

      const httpOptions = {
          headers: headers_object
      };
      //let headers = new HttpHeaders()
      //headers = headers.set('Authorization', `Bearer ${currentUser}`);

    return Observable.create(function subscribe(observer) {
        parentRef.http.post<HttpResponseObj>(url, requestObj, httpOptions)
        .pipe(
              catchError(parentRef.handleError('getHeroes', []))
             )
          .subscribe((httpEvent) => {
          if(httpEvent['errors'] == null)
          {
          observer.next(httpEvent);
          observer.complete();
          }
          else
          {
              //show validation errors or business errors
              observer.complete();
          }
        });
      });
  }



//REST API GET ALL RECORDS
public getAll<T extends BaseContainer>(url)
:Observable<T>
{
  var parentRef = this;
  //create new observable of the return type which will be executed as completed
  //as and when we get back the response from http

   //this.httpOptions.headers.append('Authorization',this.location);

  return Observable.create(function subscribe(observer) {
      parentRef.http.get<HttpResponseObj>(url,parentRef.httpOptions)
      .pipe(
            catchError(parentRef.handleError('getHeroes', []))
           )
        .subscribe((httpEvent) => {
        if(httpEvent['errors'] == null)
        {
        observer.next(httpEvent);
        observer.complete();
        }
        else
        {
            //show validation errors or business errors
            observer.complete();
        }
      });
    });
}


//REST API GET SPECIFIC RECORD BASED ON ID
public find<T extends BaseContainer>(id : any , url)
:Observable<T>
{
  var parentRef = this;

  //create new observable of the return type which will be executed as completed
  //as and when we get back the response from http

  return Observable.create(function subscribe(observer) {
      parentRef.http.get<HttpResponseObj>(url+'/'+id)
      .pipe(
            catchError(parentRef.handleError('getHeroes', []))
           )
        .subscribe((httpEvent) => {
        if(httpEvent['errors'] == null)
        {
        observer.next(httpEvent);
        observer.complete();
        }
        else
        {
            //show validation errors or business errors
            observer.complete();
        }
      });
    });
}


//CREATE A NEW RECORD
public save<T extends BaseContainer>(requestObj , url)
:Observable<T>
{
  var parentRef = this;
  var httpRequestObj = new HttpRequestObj(requestObj);

  //create new observable of the return type which will be executed as completed
  //as and when we get back the response from http

  return Observable.create(function subscribe(observer) {
      parentRef.http.post<HttpResponseObj>(url, httpRequestObj,parentRef.httpOptions)
      .pipe(
            catchError(parentRef.handleError('getHeroes', []))
           )
      .subscribe((httpEvent) => {
        if (httpEvent['errors'] == null)
        {
        observer.next(httpEvent);
        observer.complete();
        }
        else
        {
            //show validation errors or business errors
            observer.complete();
        }
      });
    });
}


//UPDATE AN EXISTING RECORD
public update<T extends BaseContainer>(requestObj ,url,id )
:Observable<T>
{
  var parentRef = this;
  var httpRequestObj = new HttpRequestObj(requestObj);

  //create new observable of the return type which will be executed as completed
  //as and when we get back the response from http

  return Observable.create(function subscribe(observer) {
      parentRef.http.put<HttpResponseObj>(url+'/'+id, httpRequestObj,parentRef.httpOptions)
      .pipe(
            catchError(parentRef.handleError('getHeroes', []))
           )
      .subscribe((httpEvent) => {
        if(httpEvent['errors'] == null)
        {
        observer.next(httpEvent);
        observer.complete();
        }
        else
        {
            //show validation errors or business errors
            observer.complete();
        }
      });
    });
}


//DELETE AN EXISTING RECORD
public delete<T extends BaseContainer>(url,id )
:Observable<T>
{
  var parentRef = this;

  //create new observable of the return type which will be executed as completed
  //as and when we get back the response from http

  return Observable.create(function subscribe(observer) {
      parentRef.http.delete<HttpResponseObj>(url+'/'+id)
      .pipe(
            catchError(parentRef.handleError('getHeroes', []))
           )
      .subscribe((httpEvent) => {
        if(httpEvent['errors'] == null)
        {
        observer.next(httpEvent);
        observer.complete();
        }
        else
        {
            //show validation errors or business errors
            observer.complete();
        }
      });
    });
}



  private handleError<T> (operation = 'operation', result?: T) {
  return (error: any): Observable<T> => {

    // TODO: send the error to remote logging infrastructure
    console.error(error); // log to console instead

    // TODO: better job of transforming error for user consumption
    //this.log(`${operation} failed: ${error.message}`);

    // Let the app keep running by returning an empty result.
    return of(result as T);
  };
}

}
