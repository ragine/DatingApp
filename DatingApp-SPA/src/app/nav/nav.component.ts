import { Component, OnInit } from '@angular/core';
import { Form } from '@angular/forms';
import { AuthService } from '../_services/auth.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {};
  constructor(private auth: AuthService) { }

  ngOnInit() {
  }

  login() {
    this.auth.login(this.model).subscribe(next => {
      console.log('Loggedin successfully');
    }, error => {
      console.log('Error in logging in');
    });
  }

  loggedIn(){
    const token = localStorage.getItem('token');
    return !!token;
  }

  logOut(){
    localStorage.removeItem('token');
    console.log('Logged Out');
  }

}
