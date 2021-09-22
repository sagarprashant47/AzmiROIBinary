import { Component ,OnInit } from '@angular/core';
import { FormGroup,FormBuilder,Validators } from '@angular/forms';
import { CustomerService } from '../../services/customer.service';
import { CommonService } from '../../services/common.service';
import { environment } from '../../../environments/environment';
import { Router, ActivatedRoute, ParamMap } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import * as $ from 'jquery';
@Component({
  templateUrl: 'withdrawal.component.html'
})
export class WithdrawalComponent implements OnInit{

withdrawal: FormGroup;
submitted = false;

constructor(private formBuilder: FormBuilder,
  private customerservice:CustomerService,
  private commonservice:CommonService,
  private router: Router,
  private toastr:ToastrService) { }

CustomerId:string = localStorage.getItem("CustomerId");
CustomerInfoModel = { BitcoinAddress : ""};
FinalAmount:number = 0;
WithdrawalFees:number = 0;
CurrencyCode:string;
PaymentProcessor=[];
ngOnInit(): void {
  this.CurrencyCode = (localStorage.getItem("CurrencyCode") == null) ? "USD" : localStorage.getItem("CurrencyCode");
  this.withdrawal =this.formBuilder.group({
    Amount: ['', Validators.required],
    ProcessorId:['', Validators.required],
    SecurityPassword: ['', Validators.required],
  });
  
  this.customerservice.GetCustomerInfo(this.CustomerId)
  .subscribe(
    res => {
      this.CustomerInfoModel = res.data;
    },
    err => console.log(err)
  )

  this.commonservice.GetPaymentProcessor()
  .subscribe(
    res =>{
      this.PaymentProcessor = res.data;
    }
  )
}

get f() { return this.withdrawal.controls; }

    onSubmit() {
        this.submitted = true;

        // stop here if form is invalid
        if (this.withdrawal.invalid) {
            return;
        }
        
        if(this.withdrawal.value.Amount < 25){
          this.toastr.error("Withdrawal Amount must be equal or greater then 25$.");
          return;
        }
        if(this.withdrawal.value.Amount > 500){
          this.toastr.error("Maximum Withdrawal Amount 500$.");
          return;
        }
        
        this.withdrawal.value.CustomerId = this.CustomerId;
        //here validate security pwd
        $('.loaderbo').show();
        this.commonservice.GetCustomerSecurityPwd(this.withdrawal.value)
        .subscribe(
          res =>{
            if(res.Message === "success"){
              debugger
              if(res.data.indexOf('Invalid') !== -1){
                this.toastr.error("Please Enter Correct Security Password")
              }
              else{
                this.commonservice.Withdrawfund(this.withdrawal.value)
                .subscribe(
                  res =>{
                    if(res.Message === "success"){
                      this.toastr.success("Your withdrawal request is received","Congratulations !!")
                      this.router.navigate(['/dashboard']);
                    }
                    else{
                      this.toastr.error(res.Message)
                    }
                    $('.loaderbo').hide();
                  },
                  err =>{
                    this.toastr.error("Something went wrong");
                    $('.loaderbo').hide();
                  }
                )
              }
            }
            else{
              this.toastr.error(res.Message)
            }
            $('.loaderbo').hide();
          },
          err =>{
            this.toastr.error("Something went wrong");
            $('.loaderbo').hide();
          }
        ) 
    }

    CalculateFees(){
      this.WithdrawalFees = 0;//environment.withdrawalFees;
      this.FinalAmount = this.withdrawal.value.Amount - this.WithdrawalFees;
    }

    onReset() {
        this.submitted = false;
        this.withdrawal.reset();
    }
}
