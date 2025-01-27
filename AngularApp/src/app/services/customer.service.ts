import { Injectable } from '@angular/core';
import { HttpClient,HttpParams } from '@angular/common/http';
import { environment } from '../../environments/environment';
@Injectable({
  providedIn: 'root'
})
export class CustomerService {

  constructor(private http: HttpClient) { }
  
  GetCustomerInfo(CustomerId)
  {
    let url = environment.baseApiUrl + "Customer/GetCustomerInfo?CustomerId="+CustomerId;
    return this.http.post<any>(url,null)
  }



  UpdateBinaryPlacementSetting(CustomerId,Value)
  {
    let url = environment.baseApiUrl + "Customer/UpdateBinaryPlacementSetting?CustomerId="+CustomerId+"&Value="+Value;
    return this.http.get<any>(url)
  }

  GetStokistDetail(UserName){
    let url = environment.baseApiUrl + "Customer/GetStokistDetail?UserName="+UserName;
    return this.http.post<any>(url,null)
  }

  GetPlanDetail(CustomerId){
    let url = environment.baseApiUrl + "Customer/GetPlanDetail?CustomerId="+CustomerId;
    return this.http.post<any>(url,null)
  }

  GetCoinRequestList(CustomerId)
  {
    let url = environment.baseApiUrl + "Customer/GetCoinRequestList?CustomerId="+CustomerId;
    return this.http.post<any>(url,null)
  }

  UpdateCustomerInfo(CustomerInfoModel)
  {
    
    let url = environment.baseApiUrl + "Customer/UpdateCustomerInfo";
    return this.http.post<any>(url,
      CustomerInfoModel
    );
  }

  SaveSecurityPassword(CustomerInfoModel)
  {
    
    let url = environment.baseApiUrl + "Customer/UpdateSecurityPwd";
    return this.http.post<any>(url,
      CustomerInfoModel
    );
  }
  
  MyReferral(CustomerId,LevelId)
  {
    let url = environment.baseApiUrl + "Customer/MyReferral?CustomerId="+CustomerId+"&LevelId="+LevelId;
    return this.http.post<any>(url,null);
  }

  GetCustomerBoard(BoardModel)
  {
    let url = environment.baseApiUrl + "Customer/GetCustomerBoard";
    return this.http.post<any>(url,BoardModel);
  }

  SubmitTicket(SupportModel){
    let url = environment.baseApiUrl + "Customer/SubmitTicket";
    return this.http.post<any>(url,SupportModel);
  }

  AddSupportTicket(SupportModel){
    let url = environment.baseApiUrl + "Ads/AddSupportTicket";
    return this.http.post<any>(url,SupportModel);
  }

  GetSupportRequest(CustomerId){
    let url = environment.baseApiUrl + "Ads/GetSupportRequest?ShowNotReplied=null&CustomerId="+CustomerId+"&PageIndex=0&MaxCount=999";
    return this.http.get<any>(url);
  }

  Setup2FA(CustomerId){
    let url = environment.baseApiUrl + "Customer/Setup2FA?CustomerId="+CustomerId;
    return this.http.get<any>(url);
  }

  Validate2FA(PinValidateRequest){
    let url = environment.baseApiUrl + "Customer/Validate2FA";
    return this.http.post<any>(url,PinValidateRequest);
  }
}
