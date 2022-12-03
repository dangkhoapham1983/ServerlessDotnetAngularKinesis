import { Component, OnInit, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { pipe } from 'rxjs';
import { Subscription, timer } from 'rxjs';
import { switchMap } from 'rxjs/operators';

@Component({
  selector: 'app-event-page',
  templateUrl: './event-page.component.html',
  styleUrls: ['./event-page.component.css']
})
export class EventPageComponent implements OnInit {
  apiUrl: string;
  eventData : any;
  subscription: Subscription;
  constructor(
    @Inject('API_URL') apiUrl: string,
    private http: HttpClient
  ) {
    this.apiUrl = apiUrl;
  }

  async ngOnInit() {

    this.subscription = timer(0, 5000).pipe(
      switchMap(async () => this.getPosts())
    ).subscribe(result => 
      console.log(result)
    );

  }
  getPosts()
  {
    this.http.get(`${this.apiUrl}api/SampleData`)
    .subscribe(
      pipe((resp: any[]) => {
        this.eventData = resp;
      })
    );
  }
  ngOnDestroy() {
    this.subscription.unsubscribe();
  }
}
