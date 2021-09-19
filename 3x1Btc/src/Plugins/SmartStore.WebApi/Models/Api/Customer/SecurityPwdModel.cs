using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Web.Mvc;

namespace SmartStore.WebApi.Models.Api.Customer
{
	public class SecurityPwdModel : ModelBase
	{
		public int CustomerId { get; set; }
		public Guid? CustomerGuid { get; set; }
		public string SecurityPassword { get; set; }
	}
}