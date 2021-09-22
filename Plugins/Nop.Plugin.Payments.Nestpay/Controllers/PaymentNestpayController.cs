using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.Nestpay.Models;
using Nop.Plugin.Payments.Nestpay.Validators;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
 


namespace Nop.Plugin.Payments.Nestpay.Controllers
{
    public class PaymentNestpayController : BasePaymentController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;

        public PaymentNestpayController(IWorkContext workContext,
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
            var nestpayPaymentSettings = _settingService.LoadSetting<NestpayPaymentSettings>(storeScope);

            var model = new ConfigurationModel();
            model.UserName = nestpayPaymentSettings.UserName;
            model.Parola = nestpayPaymentSettings.Parola;
            model.ClientID = nestpayPaymentSettings.ClientID;
            model.Domain = nestpayPaymentSettings.Domain;
            model.Domain3D = nestpayPaymentSettings.Domain3D;
            // model.MagazaID = nestpayPaymentSettings.MagazaID;
            // model.TerminalID = nestpayPaymentSettings.TerminalID;         
            model.Key = nestpayPaymentSettings.Key;
            model.UseSandbox = nestpayPaymentSettings.UseSandbox;
            model.TransactModeId = Convert.ToInt32(nestpayPaymentSettings.TransactMode);
            //model.TransactionKey = nestpayPaymentSettings.TransactionKey;
            // model.LoginId = nestpayPaymentSettings.LoginId;
            model.AdditionalFee = nestpayPaymentSettings.AdditionalFee;
            model.AdditionalFeePercentage = nestpayPaymentSettings.AdditionalFeePercentage;
            model.TransactModeValues = nestpayPaymentSettings.TransactMode.ToSelectList();

            model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {//benim eklediğim
                model.UserName_OverrideForStore = _settingService.SettingExists(nestpayPaymentSettings, x => x.UserName, storeScope);
                model.Parola_OverrideForStore = _settingService.SettingExists(nestpayPaymentSettings, x => x.Parola, storeScope);
                // model.MagazaID_OverrideForStore = _settingService.SettingExists(nestpayPaymentSettings, x => x.MagazaID, storeScope);
                // model.TerminalID_OverrideForStore = _settingService.SettingExists(nestpayPaymentSettings, x => x.TerminalID, storeScope);
                model.ClientID_OverrideForStore = _settingService.SettingExists(nestpayPaymentSettings, x => x.ClientID, storeScope);
                model.Key_OverrideForStore = _settingService.SettingExists(nestpayPaymentSettings, x => x.Key, storeScope);
                model.Domain_OverrideForStore = _settingService.SettingExists(nestpayPaymentSettings, x => x.Domain, storeScope);
                model.Domain3D_OverrideForStore = _settingService.SettingExists(nestpayPaymentSettings, x => x.Domain3D, storeScope);

                //eklediğim kofigure




                model.UseSandbox_OverrideForStore = _settingService.SettingExists(nestpayPaymentSettings, x => x.UseSandbox, storeScope);
                model.TransactModeId_OverrideForStore = _settingService.SettingExists(nestpayPaymentSettings, x => x.TransactMode, storeScope);
                //model.TransactionKey_OverrideForStore = _settingService.SettingExists(nestpayPaymentSettings, x => x.TransactionKey, storeScope);
                // model.LoginId_OverrideForStore = _settingService.SettingExists(nestpayPaymentSettings, x => x.LoginId, storeScope);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(nestpayPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(nestpayPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
            }

            return View("~/Plugins/Payments.Nestpay/Views/PaymentNestpay/Configure.cshtml", model);
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
            var nestpayPaymentSettings = _settingService.LoadSetting<NestpayPaymentSettings>(storeScope);





            //save settings
            nestpayPaymentSettings.UserName = model.UserName;
            nestpayPaymentSettings.Parola = model.Parola;

            //nestpayPaymentSettings.UserName = model.MagazaID;
            //nestpayPaymentSettings.UserName = model.TerminalID;
            nestpayPaymentSettings.ClientID = model.ClientID;
            nestpayPaymentSettings.Key = model.Key;
            nestpayPaymentSettings.Domain = model.Domain;
            nestpayPaymentSettings.Domain3D = model.Domain3D;
            nestpayPaymentSettings.UseSandbox = model.UseSandbox;
            nestpayPaymentSettings.TransactMode = (TransactMode)model.TransactModeId;
            //nestpayPaymentSettings.TransactionKey = model.TransactionKey;
            //  nestpayPaymentSettings.LoginId = model.LoginId;
            nestpayPaymentSettings.AdditionalFee = model.AdditionalFee;
            nestpayPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */

            if (model.UserName_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(nestpayPaymentSettings, x => x.UserName, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(nestpayPaymentSettings, x => x.UserName, storeScope);
            if (model.Parola_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(nestpayPaymentSettings, x => x.Parola, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(nestpayPaymentSettings, x => x.Parola, storeScope);
            if (model.ClientID_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(nestpayPaymentSettings, x => x.ClientID, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(nestpayPaymentSettings, x => x.ClientID, storeScope);
            if (model.Key_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(nestpayPaymentSettings, x => x.Key, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(nestpayPaymentSettings, x => x.Key, storeScope);
            if (model.Domain_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(nestpayPaymentSettings, x => x.Domain, storeScope, false);
            else if (storeScope > 0)
                if (model.Domain3D_OverrideForStore || storeScope == 0)
                    _settingService.SaveSetting(nestpayPaymentSettings, x => x.Domain3D, storeScope, false);
                else if (storeScope > 0)
                    _settingService.DeleteSetting(nestpayPaymentSettings, x => x.Domain3D, storeScope);
            if (model.UseSandbox_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(nestpayPaymentSettings, x => x.UseSandbox, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(nestpayPaymentSettings, x => x.UseSandbox, storeScope);

            if (model.UseSandbox_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(nestpayPaymentSettings, x => x.UseSandbox, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(nestpayPaymentSettings, x => x.UseSandbox, storeScope);

            if (model.TransactModeId_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(nestpayPaymentSettings, x => x.TransactMode, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(nestpayPaymentSettings, x => x.TransactMode, storeScope);

            //if (model.TransactionKey_OverrideForStore || storeScope == 0)
            //    _settingService.SaveSetting(nestpayPaymentSettings, x => x.TransactionKey, storeScope, false);
            //else if (storeScope > 0)
            //    _settingService.DeleteSetting(nestpayPaymentSettings, x => x.TransactionKey, storeScope);

            //if (model.LoginId_OverrideForStore || storeScope == 0)
            //    _settingService.SaveSetting(nestpayPaymentSettings, x => x.LoginId, storeScope, false);
            //else if (storeScope > 0)
            //    _settingService.DeleteSetting(nestpayPaymentSettings, x => x.LoginId, storeScope);

            if (model.AdditionalFee_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(nestpayPaymentSettings, x => x.AdditionalFee, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(nestpayPaymentSettings, x => x.AdditionalFee, storeScope);

            if (model.AdditionalFeePercentage_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(nestpayPaymentSettings, x => x.AdditionalFeePercentage, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(nestpayPaymentSettings, x => x.AdditionalFeePercentage, storeScope);

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

            return View("~/Plugins/Payments.Nestpay/Views/PaymentNestpay/PaymentInfo.cshtml", model);
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
