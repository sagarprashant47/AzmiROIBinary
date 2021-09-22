import { Component } from '@angular/core';
import { CommonService } from '../../services/common.service';
import * as $ from 'jquery';
import { ToastrService } from 'ngx-toastr';
import { CustomerService } from '../../services/customer.service';

@Component({
  templateUrl: 'transferfund.component.html'
})
export class TransferFundComponent {

  constructor(private commonservice:CommonService,
    private customerservice:CustomerService,
    private toastr:ToastrService) { }
  CustomerId:string = localStorage.getItem("CustomerId");
  CustomerEmail="";
  CustomerInfoModel = { BitcoinAddress : "",AvailableBalance:""};
  Amount=0;
  securityPwd="";
  ngOnInit(): void {
    $('.loaderbo').show();
    this.customerservice.GetCustomerInfo(this.CustomerId)
    .subscribe(
      res => {
        this.CustomerInfoModel = res.data;
        $('.loaderbo').hide();
      },
      err => console.log(err)
    )
  }

  transferfund(){
    let model = { CustomerId : this.CustomerId,CustomerEmail:"",Amount:0 ,SecurityPwd:""};
    model.CustomerEmail = this.CustomerEmail;
    model.Amount = this.Amount;
    model.CustomerId = this.CustomerId;
    model.SecurityPwd = this.securityPwd;

    if(model.CustomerEmail == ""){
      this.toastr.error("Please enter Username");
      return;
    }
    if(model.Amount <= 0){
      this.toastr.error("Please enter Amount");
      return;
    }
    if(model.Amount > 100){
      this.toastr.error("Maximum Transfer 100$ Per Day.");
      return;
    }
    if(model.SecurityPwd == ""){
      this.toastr.error("Please enter Security Password");
      return;
    }

    $('.loaderbo').show();
    let SecModel = { CustomerId : this.CustomerId,CustomerEmail:"",SecurityPassword:model.SecurityPwd};
     this.commonservice.GetCustomerSecurityPwd(SecModel)
      .subscribe(
        res =>{
          if(res.Message === "success"){
            debugger
            if(res.data.indexOf('Invalid') !== -1){
              this.toastr.error("Please Enter Correct Security Password")
            }
            else{
              this.commonservice.TransferFund(model)
              .subscribe(
                res => {
                  if(res.Message == "success"){
                    this.CustomerEmail= "";
                    this.Amount = 0;
                    this.toastr.success("Transfer Successfull");
                    this.ngOnInit();
                  }
                  else{
                    this.toastr.success(res.Message);
                  }
                  $('.loaderbo').hide();
                },
                err => {
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
}
