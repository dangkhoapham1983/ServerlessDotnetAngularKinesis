import { Component, OnInit } from '@angular/core';
import { includes } from 'lodash';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'Events Receive';
  showSignIn = false;

  constructor(
  ) {
  }

  async ngOnInit() {
  }
}
