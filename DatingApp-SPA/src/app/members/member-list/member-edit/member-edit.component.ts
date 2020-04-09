import { Component, OnInit, ViewChild } from '@angular/core';
import { User } from 'src/app/_models/user';
import { ActivationEnd, ActivatedRoute } from '@angular/router';
import { AlertifyService } from 'src/app/_services/alertify.service';
import { NgForm } from '@angular/forms';
import { AuthService } from 'src/app/_services/auth.service';
import { UserService } from 'src/app/_services/user.service';

@Component({
  selector: 'app-member-edit',
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.css']
})
export class MemberEditComponent implements OnInit {
user: User;
@ViewChild('editForm', {static: true}) editForm: NgForm;
  constructor(private router: ActivatedRoute, private alertify: AlertifyService,
              private authService: AuthService, private userService: UserService) { }

  ngOnInit() {
    this.router.data.subscribe(data => {
      this.user = data['user'];
    });
  }

  updateUser() {
    this.userService.updateUser(this.authService.decodedToken.nameid, this.user).subscribe(next => {
      this.alertify.success('Profile Updated');
      this.editForm.reset(this.user);
      }, error => {
        this.alertify.error(error);
    });
  }
}
