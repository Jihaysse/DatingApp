import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-test-errors',
  templateUrl: './test-errors.component.html',
  styleUrls: ['./test-errors.component.css']
})
export class TestErrorsComponent implements OnInit {
  baseUrl = 'https://localhost:5001/api/';
  validationErrors: string[] = [];

  constructor(private httpClient: HttpClient) { }

  ngOnInit(): void {
  }

  get404Error() {
    this.httpClient.get(this.baseUrl + 'buggy/not-found').subscribe(
      {
        next: (response) => {
          console.log(response);
        },
        error: (error) => {
          console.log(error);
        }
      }
    )
  }

  get400Error() {
    this.httpClient.get(this.baseUrl + 'buggy/bad-request').subscribe(
      {
        next: (response) => {
          console.log(response);
        },
        error: (error) => {
          console.log(error);
        }
      }
    )
  }

  get500Error() {
    this.httpClient.get(this.baseUrl + 'buggy/server-error').subscribe(
      {
        next: (response) => {
          console.log(response);
        },
        error: (error) => {
          console.log(error);
        }
      }
    )
  }

  get401Error() {
    this.httpClient.get(this.baseUrl + 'buggy/auth').subscribe(
      {
        next: (response) => {
          console.log(response);
        },
        error: (error) => {
          console.log(error);
        }
      }
    )
  }

  get400ValidationError() {
    this.httpClient.post(this.baseUrl + 'account/register', {}).subscribe(
      {
        next: (response) => {
          console.log(response);
        },
        error: (error) => {
          console.log(error);
          this.validationErrors = error;
        }
      }
    )
  }

}
