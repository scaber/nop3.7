using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.Finans.Models;
using Nop.Plugin.Payments.Finans.Validators;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
 


namespace Nop.Plugin.Payments.Finans.Controllers
{
    public class PaymentFinansController : BasePaymentController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;

        public PaymentFinansController(IWorkContext workContext,
            IStoreService storeService,
            ISettingService settingService,
            ILocalizationService localizationService)
        {
            this._workContext = workContext;
            this._storeService = storeService;
            this._settingService = settingService;
            this._localizationService = localizationService;
        }
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {




            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var finansPaymentSettings = _settingService.LoadSetting<FinansPaymentSettings>(storeScope);

            var model = new ConfigurationModel();
            model.MbrId = finansPaymentSettings.MbrId;
            model.MerchantID = finansPaymentSettings.MerchantID;
            model.MerchantPass = finansPaymentSettings.MerchantPass;
            model.UserCode = finansPaymentSettings.UserCode;
            model.OkUrl = finansPaymentSettings.OkUrl;
              
            model.FailUrl = finansPaymentSettings.FailUrl;
            model.ApiFinans = finansPaymentSettings.ApiFinans;
         
            model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {//benim eklediğim
                model.MbrId_OverrideForStore = _settingService.SettingExists(finansPaymentSettings, x => x.MbrId, storeScope);
                model.MerchantID_OverrideForStore = _settingService.SettingExists(finansPaymentSettings, x => x.MerchantID, storeScope);
              
                model.MerchantPass_OverrideForStore = _settingService.SettingExists(finansPaymentSettings, x => x.MerchantPass, storeScope);
                model.UserCode_OverrideForStore = _settingService.SettingExists(finansPaymentSettings, x => x.UserCode, storeScope);
                model.OkUrl_OverrideForStore = _settingService.SettingExists(finansPaymentSettings, x => x.OkUrl, storeScope);
                model.FailUrl_OverrideForStore = _settingService.SettingExists(finansPaymentSettings, x => x.FailUrl, storeScope);
                model.ApiFinans_OverrideForStore = _settingService.SettingExists(finansPaymentSettings, x => x.ApiFinans, storeScope); 
            }

            return View("~/Plugins/Payments.Finans/Views/PaymentFinans/Configure.cshtml", model);
        }
        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var finansPaymentSettings = _settingService.LoadSetting<FinansPaymentSettings>(storeScope); 

            //save settings
            finansPaymentSettings.MbrId = model.MbrId;
            finansPaymentSettings.MerchantID = model.MerchantID; 
            finansPaymentSettings.MerchantPass = model.MerchantPass;
            finansPaymentSettings.UserCode = model.UserCode;
            finansPaymentSettings.OkUrl = model.OkUrl;
            finansPaymentSettings.FailUrl = model.FailUrl;
            finansPaymentSettings.ApiFinans = model.ApiFinans;
          
            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */

            if (model.MbrId_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(finansPaymentSettings, x => x.MbrId, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(finansPaymentSettings, x => x.MbrId, storeScope);
            if (model.MerchantID_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(finansPaymentSettings, x => x.MerchantID, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(finansPaymentSettings, x => x.MerchantID, storeScope);
            if (model.MerchantPass_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(finansPaymentSettings, x => x.MerchantPass, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(finansPaymentSettings, x => x.MerchantPass, storeScope);
            if (model.UserCode_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(finansPaymentSettings, x => x.UserCode, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(finansPaymentSettings, x => x.UserCode, storeScope);
            
            if (model.OkUrl_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(finansPaymentSettings, x => x.OkUrl, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(finansPaymentSettings, x => x.OkUrl, storeScope);

            if (model.FailUrl_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(finansPaymentSettings, x => x.FailUrl, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(finansPaymentSettings, x => x.FailUrl, storeScope);

            if (model.ApiFinans_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(finansPaymentSettings, x => x.ApiFinans, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(finansPaymentSettings, x => x.ApiFinans, storeScope); 
            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            var model = new PaymentInfoModel();
            model.CreditCardTypes.Add(new SelectListItem
            {
                Text = "Visa",
                Value = "Visa",
            });
            model.CreditCardTypes.Add(new SelectListItem
            {
                Text = "Master card",
                Value = "MasterCard",
            });
            //years
            for (int i = 0; i < 15; i++)
            {
                string year = Convert.ToString(DateTime.Now.Year + i);
                model.ExpireYears.Add(new SelectListItem
                {
                    Text = year,
                    Value = year,
                });
            }

            //months
            for (int i = 1; i <= 12; i++)
            {
                string text = (i < 10) ? "0" + i : i.ToString();
                model.ExpireMonths.Add(new SelectListItem
                {
                    Text = text,
                    Value = i.ToString(),
                });
            }

            //set postback values
            var form = this.Request.Form;
            var selectedCcType = model.CreditCardTypes.FirstOrDefault(x => x.Value.Equals(form["CreditCardType"], StringComparison.InvariantCultureIgnoreCase));
            if (selectedCcType != null)
                selectedCcType.Selected = true;
            model.CardholderName = form["CardholderName"];
            model.CardNumber = form["CardNumber"];
            model.CardCode = form["CardCode"];
            var selectedMonth = model.ExpireMonths.FirstOrDefault(x => x.Value.Equals(form["ExpireMonth"], StringComparison.InvariantCultureIgnoreCase));
            if (selectedMonth != null)
                selectedMonth.Selected = true;
            var selectedYear = model.ExpireYears.FirstOrDefault(x => x.Value.Equals(form["ExpireYear"], StringComparison.InvariantCultureIgnoreCase));
            if (selectedYear != null)
                selectedYear.Selected = true;

            return View("~/Plugins/Payments.Finans/Views/PaymentFinans/PaymentInfo.cshtml", model);
        }
     
        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();

            //validate
            var validator = new PaymentInfoValidator(_localizationService);
            var model = new PaymentInfoModel
            {
               // CreditCardType = form["CreditCardType"],
                CardholderName = form["CardholderName"],
                CardNumber = form["CardNumber"],
                CardCode = form["CardCode"],
                ExpireMonth = form["ExpireMonth"],
                ExpireYear = form["ExpireYear"]
            };
            var validationResult = validator.Validate(model);
            if (!validationResult.IsValid)
                foreach (var error in validationResult.Errors)
                    warnings.Add(error.ErrorMessage);
            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            paymentInfo.CreditCardType = form["CreditCardType"];
            paymentInfo.CreditCardName = form["CardholderName"];
            paymentInfo.CreditCardNumber = form["CardNumber"];
            paymentInfo.CreditCardExpireMonth = int.Parse(form["ExpireMonth"]);
            paymentInfo.CreditCardExpireYear = int.Parse(form["ExpireYear"]);
            paymentInfo.CreditCardCvv2 = form["CardCode"];
            return paymentInfo;
        }
    }
}
