import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister=new EventEmitter();
  model:any={}

  constructor(private accountservice:AccountService,private router:Router,private toastr:ToastrService) { }

  ngOnInit(): void {
  }
  register()
  {
    this.accountservice.register(this.model).subscribe(response =>{
      this.router.navigateByUrl('/members');
      this.cancel();
    },error=>
    {
     this.toastr.error(error.error); 
      console.log(error)
    })
  }
  cancel()
  {
    this.cancelRegister.emit(false);
    this.router.navigateByUrl('/');
  }

}
