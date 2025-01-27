﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartStore.Core;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SmartStore.Services;
using SmartStore.Services.Authentication;
using SmartStore.Services.Authentication.External;
using SmartStore.Services.Catalog;
using SmartStore.Services.Catalog.Extensions;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Forums;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Messages;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Services.Security;
using SmartStore.Services.Tax;
using SmartStore.Web.Framework.Plugins;
using SmartStore.WebApi.Models.Api.Customer;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using SmartStore.Core.Domain.Hyip;
using System.Web.Mvc;
using System.Collections.Generic;
using SmartStore.Services.Boards;
using SmartStore.Services.Hyip;
using SmartStore.Admin.Models.Investment;
using SmartStore.Admin.Models.PaymentMethods;
using SmartStore.Admin;
using Telerik.Web.Mvc;
using SmartStore.Web.Framework.WebApi.Security;
using SmartStore.Admin.Models.Board;
using SmartStore.Web.Models.Common;
using SmartStore.Core.Email;
using SmartStore.Services.Advertisments;
using Twilio.Rest.Api.V2010.Account;
using Twilio;

namespace SmartStore.WebApi.Controllers.Api
{
	[CustomWebApiAuthenticate]
	public class CustomerController : ApiController
	{
		#region Fields
		public Localizer T { get; set; }//Added by Yagnesh 
		public ICommonServices Services { get; set; }//Added by Yagnesh 
		private readonly ICommonServices _services;
		private readonly IAuthenticationService _authenticationService;
		private readonly IDateTimeHelper _dateTimeHelper;
		private readonly DateTimeSettings _dateTimeSettings;
		private readonly ILocalizationService _localizationService;
		private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
		private readonly ICustomerService _customerService;
		private readonly IGenericAttributeService _genericAttributeService;
		private readonly ICustomerRegistrationService _customerRegistrationService;
		private readonly ITaxService _taxService;
		private readonly CustomerSettings _customerSettings;
		private readonly ICurrencyService _currencyService;
		private readonly IPriceFormatter _priceFormatter;
		private readonly IPictureService _pictureService;
		private readonly IOpenAuthenticationService _openAuthenticationService;
		private readonly IDownloadService _downloadService;
		private readonly IWebHelper _webHelper;
		private readonly ICustomerActivityService _customerActivityService;
		private readonly MediaSettings _mediaSettings;
		private readonly LocalizationSettings _localizationSettings;
		private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
		private readonly PluginMediator _pluginMediator;
		private readonly IPermissionService _permissionService;
		private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
		private readonly IBoardService _boardService;
		private readonly IPlanService _planService;
		private readonly ICustomerPlanService _customerPlanService;
		private readonly ITransactionService _transactionService;
		private readonly ICountryService _countryService;
		private readonly IAuthenticationService _formsAuthenticationService;
		private readonly IAdCampaignService _adCampaignService;
		#endregion

		#region Ctor

		public CustomerController(
			ITransactionService transactionService,
			ICustomerPlanService customerPlanService,
			IPlanService planService,
			ICommonServices services,
			IAuthenticationService authenticationService,
			IDateTimeHelper dateTimeHelper,
			DateTimeSettings dateTimeSettings, TaxSettings taxSettings,
			ILocalizationService localizationService,
			IWorkContext workContext, IStoreContext storeContext,
			ICustomerService customerService,
			IGenericAttributeService genericAttributeService,
			ICustomerRegistrationService customerRegistrationService,
			ITaxService taxService, RewardPointsSettings rewardPointsSettings,
			CustomerSettings customerSettings, AddressSettings addressSettings, ForumSettings forumSettings,
			ICurrencyService currencyService,
			IPriceFormatter priceFormatter,
			IPictureService pictureService, INewsLetterSubscriptionService newsLetterSubscriptionService,
			ICustomerActivityService customerActivityService,
			MediaSettings mediaSettings,
			LocalizationSettings localizationSettings,
			ExternalAuthenticationSettings externalAuthenticationSettings,
			PluginMediator pluginMediator,
			IPermissionService permissionService,
			IBoardService boardService,
			ICountryService countryService,
			IAuthenticationService formsAuthenticationService,
			IAdCampaignService adCampaignService)
		{
			_planService = planService;
			_customerPlanService = customerPlanService;
			_transactionService = transactionService;
			_services = services;
			_authenticationService = authenticationService;
			_dateTimeHelper = dateTimeHelper;
			_dateTimeSettings = dateTimeSettings;
			_localizationService = localizationService;
			_workContext = workContext;
			_storeContext = storeContext;
			_customerService = customerService;
			_genericAttributeService = genericAttributeService;
			_customerRegistrationService = customerRegistrationService;
			_taxService = taxService;
			_customerSettings = customerSettings;
			_currencyService = currencyService;
			_priceFormatter = priceFormatter;
			_pictureService = pictureService;
			_newsLetterSubscriptionService = newsLetterSubscriptionService;
			_customerActivityService = customerActivityService;
			_mediaSettings = mediaSettings;
			_localizationSettings = localizationSettings;
			_externalAuthenticationSettings = externalAuthenticationSettings;
			_pluginMediator = pluginMediator;
			_permissionService = permissionService;
			_boardService = boardService;
			_countryService = countryService;
			_formsAuthenticationService = formsAuthenticationService;
			_adCampaignService = adCampaignService;
		}

		#endregion

		[System.Web.Http.HttpPost]
		[System.Web.Http.ActionName("MyReferral")]
		public HttpResponseMessage MyReferral(int CustomerId, int LevelId)
		{
			var customerguid = Request.Headers.GetValues("CustomerGUID").FirstOrDefault();
			if (customerguid != null)
			{
				var cust = _customerService.GetCustomerByGuid(Guid.Parse(customerguid));
				if (CustomerId != cust.Id)
				{
					return Request.CreateResponse(HttpStatusCode.Unauthorized, new { code = 0, Message = "something went wrong" });
				}
			}
			var gridModel = new GridModel<TempReferralList>();
			var customerReferral = _customerService.GetCustomerReferral(CustomerId).Where(x => x.LevelId == LevelId).ToList();
			gridModel.Data = customerReferral.Select(x =>
			{
				var myReferral = new TempReferralList();
				myReferral.LevelId = x.LevelId;
				myReferral.CustomerId = x.CustomerId;
				myReferral.EmailId = x.EmailId;
				myReferral.AmountInvested = x.AmountInvested + " " + _workContext.WorkingCurrency.CurrencyCode;
				myReferral.IsPaid = x.IsPaid;
				myReferral.MatrixPaid = x.MatrixPaid;
				myReferral.ReferredBy = x.ReferredBy;
				myReferral.RegistrationDate = x.RegistrationDate;
				myReferral.PlanName = x.PlanName;
				return myReferral;
			});

			gridModel.Total = customerReferral.Count();

			return Request.CreateResponse(HttpStatusCode.OK, new { code = 0, Message = "success", data = gridModel });
		}

		[System.Web.Http.HttpPost]
		[System.Web.Http.ActionName("GetCustomerInfo")]
		public HttpResponseMessage GetCustomerInfo(int CustomerId)
		{
			var customerguid = Request.Headers.GetValues("CustomerGUID").FirstOrDefault();
			if (customerguid != null)
			{
				var cust = _customerService.GetCustomerByGuid(Guid.Parse(customerguid));
				if (CustomerId != cust.Id)
				{
					return Request.CreateResponse(HttpStatusCode.Unauthorized, new { code = 0, Message = "something went wrong" });
				}
			}
			CustomerInfoModel model = new CustomerInfoModel();
			var customer = _customerService.GetCustomerById(CustomerId);
			model.SystemName = customer.SystemName;
			model.BitcoinAddress = customer.GetAttribute<string>(SystemCustomerAttributeNames.BitcoinAddressAcc);
			model.BankName = customer.GetAttribute<string>(SystemCustomerAttributeNames.BankName);
			model.AccountHolderName = customer.GetAttribute<string>(SystemCustomerAttributeNames.AccountHolderName);
			model.AccountNumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.AccountNumber);
			model.NICR = customer.GetAttribute<string>(SystemCustomerAttributeNames.NICR);
			model.Phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
			model.PayzaAcc = customer.GetAttribute<string>(SystemCustomerAttributeNames.PayzaAcc);
			model.SolidTrustPayAcc = customer.GetAttribute<string>(SystemCustomerAttributeNames.SolidTrustPayAcc);
			model.PayeerAcc = customer.GetAttribute<string>(SystemCustomerAttributeNames.PayeerAcc);
			model.PMAcc = customer.GetAttribute<string>(SystemCustomerAttributeNames.PMAcc);
			model.AdvanceCashAcc = customer.GetAttribute<string>(SystemCustomerAttributeNames.AdvanceCashAcc);
			model.Enable2FA = customer.GetAttribute<bool>(SystemCustomerAttributeNames.Enable2FA);
			model.AvailableBalance = (float)Math.Round(_customerService.GetAvailableBalance(customer.Id), 2);
			model.TokenBalance = (float)Math.Round(_customerService.GetTokenBalance(customer.Id), 2);
			//model.NetworkIncome = _customerService.GetNetworkIncome(customer.Id);
			model.TradeIncome = _customerService.GetTradeIncome(customer.Id);
			model.TotalTransfer = _customerService.GetCustomerTransfer(customer.Id);
			model.PendingWithdrawal = _customerService.GetCustomerPendingWithdrawal(customer.Id);
			model.CompletedWithdrawal = _customerService.GetCustomerCompletedWithdrawal(customer.Id);
			model.ReferralLink = _storeContext.CurrentStore.Url + "?/#/r=" + customer.Id;
			var id = customer.Id;
			model.CompletedWithdrawal = _customerService.GetCustomerCompletedWithdrawal(id);
			model.PendingWithdrawal = _customerService.GetCustomerPendingWithdrawal(id);
			model.CyclerIncome = (float)Math.Round(_customerService.GetCustomerCyclerBonus(id), 2);
			model.DirectBonus = _customerService.GetCustomerDirectBonus(id);
			model.UnilevelEarning = _customerService.GetCustomerUnilevelBonus(id);
			model.TotalReferral = _customerService.GetCustomerPaidDirectReferral(id).Count();
			model.AdCredit = _customerService.GetAvailableCredits(CustomerId).FirstOrDefault().AvailableClick;
			model.TodaysPair = _customerService.GetCurrentActivePlan(customer.Id).ToString();
			model.InvestorId = id.ToString();
			model.Name = customer.GetFullName();
			model.SecurityPwd = customer.GetAttribute<string>(SystemCustomerAttributeNames.SecurityPassword); ;
			var ReferredBy = _customerService.GetCustomerById(customer.AffiliateId);
			if (ReferredBy != null)
			{
				model.ReferredBy = ReferredBy.GetFullName();
				if (model.ReferredBy.IsEmpty())
				{
					model.ReferredBy = ReferredBy.Email;
				}
			}
			model.RegistrationDate = customer.CreatedOnUtc.ToLongDateString();
			model.ServerTime = DateTime.Now.ToLongTimeString();
			model.Status = (customer.CustomerPosition.Count() > 0 || customer.CustomerPlan.Count() > 0) ? "Active" : "Inactive";
			model.AffilateId = customer.AffiliateId;
			model.VacationModelExpiryDate = customer.GetAttribute<DateTime>(SystemCustomerAttributeNames.VacationModeExpiryDate).ToShortDateString();
			model.NextSurfTime = customer.GetAttribute<DateTime>(SystemCustomerAttributeNames.NextSurfDate);
			if (model.NextSurfTime != DateTime.MinValue)
			{
				model.NoOfSecondsToSurf = (model.NextSurfTime - DateTime.Now).Seconds;
			}
			else
			{
				model.NoOfSecondsToSurf = 86400;
			}
			var lastsurfeddate = customer.GetAttribute<DateTime>(SystemCustomerAttributeNames.LastSurfDate);

			if (lastsurfeddate.Date < DateTime.Today)
			{
				model.NoOfAdsToSurf = 10;
			}
			else
			{
				model.NoOfAdsToSurf = customer.GetAttribute<int>(SystemCustomerAttributeNames.NoOfAdsSurfed);
				int ads = (10 - model.NoOfAdsToSurf);
				if (ads < 0)
				{
					model.NoOfAdsToSurf = 0;
				}
				else
				{
					model.NoOfAdsToSurf = (10 - model.NoOfAdsToSurf);
				}
			}
			var vacationdate = customer.GetAttribute<DateTime>(SystemCustomerAttributeNames.VacationModeExpiryDate);
			if (vacationdate > DateTime.Today)
			{
				model.NoOfAdsToSurf = 0;
			}

			model.CurrencyCode = _workContext.WorkingCurrency.CurrencyCode;
			model.CustomerId = customer.Id;
			model.CustomerGuid = customer.CustomerGuid;
			model.Username = customer.Username;
			model.Email = customer.Email;
			model.Active = customer.Active;
			model.StokistCommision = customer.GetAttribute<string>(SystemCustomerAttributeNames.StokistCommision);
			model.Gender = customer.GetAttribute<string>(SystemCustomerAttributeNames.Gender);
			model.FirstName = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName);
			model.LastName = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName);
			model.Phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
			model.CountryId = customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId);
			model.FullName = customer.GetFullName();
			model.PlacementId = customer.PlacementId;
			model.PlacementUserName = customer.PlacementUserName;
			model.Position = customer.Position;
			var currentPlan = _customerService.GetCurrentFullPlan(customer.Id);
			if(currentPlan != null)
			{
				model.PackageName = currentPlan?.Name;
				model.ROILimit = currentPlan?.MaximumInvestment.ToString();
			}
			else
			{
				model.PackageName = "";
				model.ROILimit = "";
			}
			ICollection<Transaction> transactions = customer.Transaction;
			model.TotalSB = transactions.Where(x => x.TranscationTypeId == 6 && x.CreatedOnUtc== DateTime.UtcNow).Sum(x => x.Amount);
			model.TotalTB = transactions.Where(x => x.TranscationTypeId == 7 && x.CreatedOnUtc == DateTime.UtcNow).Sum(x => x.Amount);

			return Request.CreateResponse(HttpStatusCode.OK, new { code = 0, Message = "success", data = model });
		}

		[System.Web.Http.HttpPost]
		[System.Web.Http.ActionName("GetCustomerSecurityPwd")]
		public HttpResponseMessage GetCustomerSecurityPwd(SecurityPwdModel securityPwdModel)
		{
			var customerguid = Request.Headers.GetValues("CustomerGUID").FirstOrDefault();
			if (customerguid != null)
			{
				var cust = _customerService.GetCustomerByGuid(Guid.Parse(customerguid));
				if (securityPwdModel.CustomerId != cust.Id)
				{
					return Request.CreateResponse(HttpStatusCode.Unauthorized, new { code = 0, Message = "something went wrong" });
				}
			}
			CustomerInfoModel model = new CustomerInfoModel();
			var customer = _customerService.GetCustomerById(securityPwdModel.CustomerId);
			model.SecurityPwd = customer.GetAttribute<string>(SystemCustomerAttributeNames.SecurityPassword); ;

			return Request.CreateResponse(HttpStatusCode.OK, new { code = 0, Message = "success", 
				data = (model.SecurityPwd != securityPwdModel.SecurityPassword)? "Invalid Password" : "Correct Password"});
		}

		[System.Web.Http.HttpPost]
		[System.Web.Http.ActionName("GetStokistDetail")]
		public HttpResponseMessage GetStokistDetail()
		{
			var cust = _customerService.GetStokistList();
			var gridModel = new GridModel<Customer>();

			gridModel.Data = cust.Select(x =>
			{
				var transModel = new Customer
				{
					Username = x.Username,
					Id = x.Id
				};
				return transModel;
			}).ToList();
			gridModel.Total = cust.Count;
			return Request.CreateResponse(HttpStatusCode.OK, new { code = 0, Message = "success", data = gridModel });
		}

		[System.Web.Http.HttpPost]
		[System.Web.Http.ActionName("GetPlanDetail")]
		public HttpResponseMessage GetPlanDetail(int CustomerId)
		{
			var customerguid = Request.Headers.GetValues("CustomerGUID").FirstOrDefault();
			if (customerguid != null)
			{
				var cust = _customerService.GetCustomerByGuid(Guid.Parse(customerguid));
				if (CustomerId != cust.Id)
				{
					return Request.CreateResponse(HttpStatusCode.Unauthorized, new { code = 0, Message = "something went wrong" });
				}
			}

			var plan = _customerService.GetCurrentPlanList(CustomerId);
			//var gridModel = new GridModel<CustomerPlan>();

			//gridModel.Total = 1;
			return Request.CreateResponse(HttpStatusCode.OK, new { code = 0, Message = "success", data = plan });

		}

		[System.Web.Http.HttpPost]
		[System.Web.Http.ActionName("GetCoinRequestList")]
		public HttpResponseMessage GetCoinRequestList(int CustomerId)
		{
			var customerguid = Request.Headers.GetValues("CustomerGUID").FirstOrDefault();
			if (customerguid != null)
			{
				var cust = _customerService.GetCustomerByGuid(Guid.Parse(customerguid));
				if (CustomerId != cust.Id)
				{
					return Request.CreateResponse(HttpStatusCode.Unauthorized, new { code = 0, Message = "something went wrong" });
				}
			}
			CoinRequestList model = new CoinRequestList();
			var customer = _customerService.GetCustomerById(CustomerId);
			model.CustomerId = customer.Id;
			model.CustomerGuid = customer.CustomerGuid;
			model.AvailableCoin = _customerService.GetAvailableCoin(CustomerId);
			model.Transaction = _transactionService.GetCoinRequest(CustomerId);
			return Request.CreateResponse(HttpStatusCode.OK, new { code = 0, Message = "success", data = model });
		}

		[System.Web.Http.HttpPost]
		[System.Web.Http.ActionName("UpdateCustomerInfo")]
		public HttpResponseMessage UpdateCustomerInfo(CustomerInfoModel model)
		{
			var customerguid = Request.Headers.GetValues("CustomerGUID").FirstOrDefault();
			var cust = _customerService.GetCustomerByGuid(Guid.Parse(customerguid));
			if (customerguid != null)
			{
				if (model.CustomerId != cust.Id)
				{
					return Request.CreateResponse(HttpStatusCode.Unauthorized, new { code = 0, Message = "something went wrong" });
				}
			}
			var Enable2FA = cust.GetAttribute<bool>(SystemCustomerAttributeNames.Enable2FA);
			if (Enable2FA)
			{
				var valid = _formsAuthenticationService.Validated2FA(cust, model.Pin2FA);
				if (!valid)
				{
					return Request.CreateResponse(HttpStatusCode.OK, new { code = 0, Message = "Incorrect 2FA Pin" });
				}
			}

			//try
			//{
			//	string accountSid = "ACc887bcf83d1f79d47ed76860c6dd288f"; //Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
			//	string authToken = "9ef4bc2a45051f3e79a85ffbb19bfd61"; //Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");

			//	TwilioClient.Init(accountSid, authToken);

			//	var message1 = MessageResource.Create(
			//		body: "Your magic bosster account info updated",
			//		from: new Twilio.Types.PhoneNumber("+17192498304"),
			//		to: new Twilio.Types.PhoneNumber(model.Phone)
			//	);
			//	if (!string.IsNullOrEmpty(message1.ErrorMessage))
			//	{
			//		return Request.CreateResponse(HttpStatusCode.OK, new { code = 0, Message = "Incorrect Phone number" });
			//	}
			//}
			//catch (Exception ex)
			//{
			//	return Request.CreateResponse(HttpStatusCode.OK, new { code = 0, Message = "Incorrect Phone number" });
			//}

			_genericAttributeService.SaveAttribute(cust, SystemCustomerAttributeNames.FirstName, model.StokistCommision);

			_genericAttributeService.SaveAttribute(cust, SystemCustomerAttributeNames.CountryId, model.CountryId);
			_genericAttributeService.SaveAttribute(cust, SystemCustomerAttributeNames.Gender, model.Gender);
			_genericAttributeService.SaveAttribute(cust, SystemCustomerAttributeNames.FirstName, model.FirstName);
			_genericAttributeService.SaveAttribute(cust, SystemCustomerAttributeNames.LastName, model.LastName);
			_genericAttributeService.SaveAttribute(cust, SystemCustomerAttributeNames.BitcoinAddressAcc, model.BitcoinAddress);
			_genericAttributeService.SaveAttribute(cust, SystemCustomerAttributeNames.Phone, model.Phone);

			_genericAttributeService.SaveAttribute(cust, SystemCustomerAttributeNames.AccountNumber, model.AccountNumber);
			_genericAttributeService.SaveAttribute(cust, SystemCustomerAttributeNames.AccountHolderName, model.AccountHolderName);
			_genericAttributeService.SaveAttribute(cust, SystemCustomerAttributeNames.NICR, model.NICR);
			_genericAttributeService.SaveAttribute(cust, SystemCustomerAttributeNames.BankName, model.BankName);

			_genericAttributeService.SaveAttribute(cust, SystemCustomerAttributeNames.PayzaAcc, model.PayzaAcc);
			_genericAttributeService.SaveAttribute(cust, SystemCustomerAttributeNames.PMAcc, model.PMAcc);
			_genericAttributeService.SaveAttribute(cust, SystemCustomerAttributeNames.SolidTrustPayAcc, model.SolidTrustPayAcc);
			_genericAttributeService.SaveAttribute(cust, SystemCustomerAttributeNames.PayeerAcc, model.PayeerAcc);
			return Request.CreateResponse(HttpStatusCode.OK, new { code = 0, Message = "success", data = model });
		}

		[System.Web.Http.HttpPost]
		[System.Web.Http.ActionName("UpdateSecurityPwd")]
		public HttpResponseMessage UpdateSecurityPwd(SecurityPwdModel model)
		{
			var customerguid = Request.Headers.GetValues("CustomerGUID").FirstOrDefault();
			var cust = _customerService.GetCustomerByGuid(Guid.Parse(customerguid));
			if (customerguid != null)
			{
				if (model.CustomerId != cust.Id)
				{
					return Request.CreateResponse(HttpStatusCode.Unauthorized, new { code = 0, Message = "something went wrong" });
				}
			} 
			_genericAttributeService.SaveAttribute(cust, SystemCustomerAttributeNames.SecurityPassword, model.SecurityPassword);
			return Request.CreateResponse(HttpStatusCode.OK, new { code = 0, Message = "success", data = model });
		}

		[System.Web.Http.HttpPost]
		[System.Web.Http.ActionName("GetCustomerBoard")]
		public HttpResponseMessage GetCustomerBoard(BoardModel model)
		{
			var customerguid = Request.Headers.GetValues("CustomerGUID").FirstOrDefault();
			if (customerguid != null)
			{
				var cust = _customerService.GetCustomerByGuid(Guid.Parse(customerguid));
				if (model.CustomerId != cust.Id)
				{
					return Request.CreateResponse(HttpStatusCode.Unauthorized, new { code = 0, Message = "something went wrong" });
				}
			}
			var gridModel = new GridModel<BoardModel>();
			DateTime? startDateValue = (model.StartDate == null) ? null : model.StartDate;
			DateTime? endDateValue = (model.EndDate == null) ? null : model.EndDate;

			int[] BoardIds = model.BoardIds.ToIntArray();
			bool IsCycled = model.IsCycled;
			int customerid = 0;
			customerid = model.CustomerId;
			var Customer = _customerService.GetCustomerById(model.CustomerId);

			var custPositions = _boardService.GetAllPosition(model.BoardId, customerid, model.IsCycled, 0, int.MaxValue);
			var currency = _workContext.WorkingCurrency.CurrencyCode;
			gridModel.Data = custPositions.Select(x =>
			{
				var myMatrix = _boardService.GetMyMatrixByPositionId(x.Id).FirstOrDefault();
				var transModel = new BoardModel
				{
					Id = x.Id,
					BoardId = x.BoardId,
					BoardName = _boardService.GetBoardById(x.BoardId).Name,
					CustomerName = x.Customer.GetFullName(),
					PurchaseDate = x.PurchaseDate,
					NoOfPositionFilled = myMatrix.PositionFilled,
					NoOfPositionRemaining = myMatrix.PositionRemaining,
					IsCycledString = (x.IsCycled) ? "Cycled" : "Active",
					PlacedUnderPositionId = x.PlacedUnderPositionId,
				};
				return transModel;
			}).ToList();

			gridModel.Total = custPositions.TotalCount;
			return Request.CreateResponse(HttpStatusCode.OK, new { code = 0, Message = "success", data = gridModel });

		}

		[System.Web.Http.HttpPost]
		[System.Web.Http.ActionName("SubmitTicket")]
		public HttpResponseMessage SubmitTicket(ContactUsModel model)
		{
			var customerguid = Request.Headers.GetValues("CustomerGUID").FirstOrDefault();
			var cust = _customerService.GetCustomerByGuid(Guid.Parse(customerguid));
			if (customerguid != null)
			{

				if (model.CustomerId != cust.Id)
				{
					return Request.CreateResponse(HttpStatusCode.Unauthorized, new { code = 0, Message = "something went wrong" });
				}
			}
			var LastReq = _adCampaignService.GetLatestSupportRequest(cust.Id);
			if (LastReq.CreatedDate.AddDays(30) >= DateTime.Now.Date)
			{
				return Request.CreateResponse(HttpStatusCode.OK, new { code = 0, Message = "Request Allowed Once in Month" });
			}
			var customer = _customerService.GetCustomerById(model.CustomerId);
			var email = model.Email.Trim();
			var fullName = model.FullName;
			var subject = T("ContactUs.EmailSubject", _services.StoreContext.CurrentStore.Name);
			var body = Core.Html.HtmlUtils.FormatText(model.Enquiry, false, true, false, false, false, false);

			// Required for some SMTP servers
			EmailAddress sender = null;
			sender = new EmailAddress(email, fullName);
			// email
			var msg = Services.MessageFactory.SendContactUsMessage(customer, email, fullName, subject, body, sender);

			if (msg?.Email?.Id != null)
			{
				model.SuccessfullySent = true;
				model.Result = T("ContactUs.YourEnquiryHasBeenSent");
				_services.CustomerActivity.InsertActivity("PublicStore.ContactUs", T("ActivityLog.PublicStore.ContactUs"));
				return Request.CreateResponse(HttpStatusCode.OK, new { code = 0, Message = "success", data = model.Result });

			}
			else
			{
				ModelState.AddModelError("", T("Common.Error.SendMail"));
				model.Result = T("Common.Error.SendMail");
				return Request.CreateResponse(HttpStatusCode.OK, new { code = 0, Message = model.Result });
			}
		}

		[System.Web.Http.HttpGet]
		[System.Web.Http.ActionName("Setup2FA")]
		public HttpResponseMessage Setup2FA(int CustomerId)
		{
			var customerguid = Request.Headers.GetValues("CustomerGUID").FirstOrDefault();
			var cust = _customerService.GetCustomerByGuid(Guid.Parse(customerguid));
			if (customerguid != null)
			{
				if (CustomerId != cust.Id)
				{
					return Request.CreateResponse(HttpStatusCode.Unauthorized, new { code = 0, Message = "something went wrong" });
				}
			}
			var googlecode = _formsAuthenticationService.SetupAuth(cust);
			return Request.CreateResponse(HttpStatusCode.OK, new { code = 0, Message = "success", data = googlecode });
		}

		[System.Web.Http.HttpPost]
		[System.Web.Http.ActionName("Validate2FA")]
		public HttpResponseMessage Validate2FA(PinValidateRequest pinValidateRequest)
		{
			var customerguid = Request.Headers.GetValues("CustomerGUID").FirstOrDefault();
			var cust = _customerService.GetCustomerByGuid(Guid.Parse(customerguid));
			if (customerguid != null)
			{
				if (pinValidateRequest.CustomerId != cust.Id)
				{
					return Request.CreateResponse(HttpStatusCode.Unauthorized, new { code = 0, Message = "something went wrong" });
				}
			}
			var isvalid = _formsAuthenticationService.Validated2FA(cust, pinValidateRequest.pin2FA);
			if (isvalid)
			{
				_genericAttributeService.SaveAttribute(cust, SystemCustomerAttributeNames.Enable2FA, pinValidateRequest.enableRequest);
				return Request.CreateResponse(HttpStatusCode.OK, new { code = 0, Message = "success", data = new { valid = isvalid, request = pinValidateRequest.enableRequest } });
			}
			return Request.CreateResponse(HttpStatusCode.OK, new { code = 0, Message = "Invlaid 2FA Pin" });
		}

		[System.Web.Http.HttpGet]
		[System.Web.Http.ActionName("UpdateBinaryPlacementSetting")]
		public HttpResponseMessage UpdateBinaryPlacementSetting(int CustomerId,string Value)
		{
			var customerguid = Request.Headers.GetValues("CustomerGUID").FirstOrDefault();
			if (customerguid != null)
			{
				var cust = _customerService.GetCustomerByGuid(Guid.Parse(customerguid));
				if (CustomerId != cust.Id)
				{
					return Request.CreateResponse(HttpStatusCode.Unauthorized, new { code = 0, Message = "something went wrong" });
				}
				cust.SystemName = Value;
				_customerService.UpdateCustomer(cust);
			}
			
			return Request.CreateResponse(HttpStatusCode.OK, new { code = 0, Message = "Binary setting updated" });
		}


	}

	public class PinValidateRequest
	{
		public int CustomerId { get; set; }
		public string pin2FA { get; set; }
		public bool enableRequest { get; set; }
	}
}