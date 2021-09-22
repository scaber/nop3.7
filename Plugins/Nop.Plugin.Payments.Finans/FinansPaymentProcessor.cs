using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Text;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.Finans.Controllers;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using System.Collections;

namespace Nop.Plugin.Payments.Finans
{
    public class FinansPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly FinansPaymentSettings _finansPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly CurrencySettings _currencySettings;
        private readonly IWebHelper _webHelper;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IEncryptionService _encryptionService;

        #endregion

        #region Ctor

        public FinansPaymentProcessor(FinansPaymentSettings FinansPaymentSettings,
            ISettingService settingService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            CurrencySettings currencySettings, IWebHelper webHelper,
            IOrderTotalCalculationService orderTotalCalculationService, IEncryptionService encryptionService)
        {
            this._finansPaymentSettings = FinansPaymentSettings;
            this._settingService = settingService;
            this._currencyService = currencyService;
            this._customerService = customerService;
            this._currencySettings = currencySettings;
            this._webHelper = webHelper;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._encryptionService = encryptionService;
        }

        #endregion
     
        #region Methods




        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        
        public static string GetIp()
        {
            if (string.IsNullOrEmpty(System.Web.HttpContext.Current.Request.ServerVariables["remote_addr"]))
            {
                return "127.0.0.1";
            }
            else
            {
                return System.Web.HttpContext.Current.Request.ServerVariables["remote_addr"].ToString();
            }
        }

        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();

            var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);

            var webClient = new WebClient();
            var form = new NameValueCollection();
            //Kurum Kodu
            string MbrId = _finansPaymentSettings.MbrId;
            //Language_MerchantID
            string MerchantID = "085300000009704";
            //Language_MerchantPass
            string MerchantPass = _finansPaymentSettings.MerchantPass;
            //Kullanici Kodu
            string UserCode = "QNB_ISYERI_KULLANICI_3DPAY";
            //Language_SecureType
            string SecureType = "3DHost";
            //Islem Tipi
            string TxnType = "Auth";
            //Taksit Sayisi
            string InstallmentCount = "0";
            //Para Birimi
            string Currency = "949";
            //Language_OkUrl
            string OkUrl = _finansPaymentSettings.OkUrl;
            //Language_FailUrl
            string FailUrl = _finansPaymentSettings.FailUrl;
            //Siparis Numarasi
            string OrderId = "";
            //Orijinal Islem Siparis Numarasi
            string OrgOrderId = "";
            //Tutar
            //Language_Lang
            string Lang = "TR";
            string rnd = DateTime.Now.Ticks.ToString();
            var orderTotal = Math.Round(processPaymentRequest.OrderTotal, 2);
            string PurchAmount = orderTotal.ToString("0.00", CultureInfo.InvariantCulture);
            string ApiFinans = _finansPaymentSettings.ApiFinans;

            form.Add("MbrId", MbrId);
            form.Add("MerchantID", _finansPaymentSettings.MerchantID);
            form.Add("MerchantPass", MerchantPass);
            form.Add("UserCode", _finansPaymentSettings.UserCode);
            form.Add("SecureType", "3DHost");
            form.Add("TxnType", "Auth");
            form.Add("InstallmentCount", "0");
            form.Add("Currency", "949");
            form.Add("OkUrl", OkUrl);
            form.Add("FailUrl", FailUrl);
            form.Add("OrderId", "");
            form.Add("OrgOrderId", "");
            form.Add("PurchAmount", PurchAmount);
            form.Add("Lang", Lang);
            form.Add("Rnd", rnd);
            string str = MbrId + OrderId + PurchAmount + OkUrl + FailUrl + TxnType + InstallmentCount + rnd + MerchantPass;

            System.Security.Cryptography.SHA1 sha = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
            byte[] hashingbytes = sha.ComputeHash(bytes);
            String hash = Convert.ToBase64String(hashingbytes);

            form.Add("CardHolderName", processPaymentRequest.CreditCardName);
            form.Add("Pan", processPaymentRequest.CreditCardNumber.ToString());
            form.Add("Cvv2", string.Format("{0:000}", processPaymentRequest.CreditCardCvv2));
            form.Add("Expiry", string.Format("{0:00}", processPaymentRequest.CreditCardExpireMonth) + processPaymentRequest.CreditCardExpireYear.ToString().Substring(2, 2));

            form.Add("Hash", hash);


            webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            var responseData = webClient.UploadValues(ApiFinans, "POST", form);
            var reply = Encoding.ASCII.GetString(responseData);
            if (!String.IsNullOrEmpty(reply))
            {
                string[] responseFields = reply.Split('|');
                switch (responseFields[0])
                {
                    case "1":
                        result.AuthorizationTransactionCode = string.Format("{0},{1}", responseFields[6], responseFields[4]);
                        result.AuthorizationTransactionResult = string.Format("Approved ({0}: {1})", responseFields[2], responseFields[3]);
                        result.AvsResult = responseFields[5];
                        //responseFields[38];

                        break;
                    case "2":
                        result.AddError(string.Format("Declined ({0}: {1})", responseFields[2], responseFields[3]));
                        break;
                    case "3":
                        result.AddError(string.Format("Error: {0}", reply));
                        break;

                }
            }
            else
            {
                result.AddError("Authorize.NET unknown error");
            }
            return result;

        }



        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //nothing
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            var result = this.CalculateAdditionalFee(_orderTotalCalculationService, cart,
                _finansPaymentSettings.AdditionalFee, _finansPaymentSettings.AdditionalFeePercentage);
            return result;
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }



        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");
            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError("Refund method not supported");
            return result;
        }
        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return result;
        }
        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();

            result.AllowStoringCreditCardNumber = true;
         
            switch (_finansPaymentSettings.TransactMode)
            {
                case TransactMode.Pending:
                    result.NewPaymentStatus = PaymentStatus.Pending;
                    break;
                case TransactMode.Authorize:
                    result.NewPaymentStatus = PaymentStatus.Authorized;
                    break;
                case TransactMode.AuthorizeAndCapture:
                    result.NewPaymentStatus = PaymentStatus.Paid;
                    break;
                default:
                    {
                        result.AddError("Not supported transaction type");
                        return result;
                    }
            }

            return result;
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            //always success
            return new CancelRecurringPaymentResult();
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //it's not a redirection payment method. So we always return false
            return false;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentFinans";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.Finans.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentFinans";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.Finans.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(PaymentFinansController);
        }

        public override void Install()
        {
            //settings
            var settings = new FinansPaymentSettings
            {
                TransactMode = TransactMode.Authorize
            };
            _settingService.SaveSetting(settings);


            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Finans.Fields.AdditionalFee", "Additional fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Finans.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers.");
         
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Finans.Fields.TransactMode", "After checkout mark payment as");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Finans.Fields.TransactMode.Hint", "Specify transaction mode.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Finans.Fields.MbrId", "Kurum Kodu");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Finans.Fields.MbrId.Hint", "KullanıcıAdı");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Finans.Fields.MerchantID", "üye İşyeri Numarası");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Finans.Fields.MerchantID.Hint", "üye İşyeri Numarası.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Finans.Fields.MerchantPass", "üye İşyeri Şifresi");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Finans.Fields.MerchantPass.Hint", "üye İşyeri Şifresi");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Finans.Fields.UserCode", "Kullanici Kodu.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Finans.Fields.UserCode.Hint", "Kullanici Kodu");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Finans.Fields.OkUrl", "OkUrl");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Finans.Fields.OkUrl.Hint", "OkUrl");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Finans.Fields.FailUrl", "FailUrl");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Finans.Fields.FailUrl.Hint", "FailUrl");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Finans.Fields.ApiFinans", "ApiFinans");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Finans.Fields.ApiFinans.Hint", "ApiFinans");

            base.Install();
        }

        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<FinansPaymentSettings>();

            //locales
            this.DeletePluginLocaleResource("Plugins.Payments.Finans.Fields.AdditionalFee");
            this.DeletePluginLocaleResource("Plugins.Payments.Finans.Fields.AdditionalFee.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Finans.Fields.AdditionalFeePercentage");
            this.DeletePluginLocaleResource("Plugins.Payments.Finans.Fields.AdditionalFeePercentage.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Finans.Fields.TransactMode");
            this.DeletePluginLocaleResource("Plugins.Payments.Finans.Fields.TransactMode.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Finans.Fields.MbrId");
            this.DeletePluginLocaleResource("Plugins.Payments.Finans.Fields.MbrId.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Finans.Fields.MerchantID");
            this.DeletePluginLocaleResource("Plugins.Payments.Finans.Fields.MerchantID.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Finans.Fields.MerchantPass");
            this.DeletePluginLocaleResource("Plugins.Payments.Finans.Fields.MerchantPass.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Finans.Fields.UserCode");
            this.DeletePluginLocaleResource("Plugins.Payments.Finans.Fields.UserCode.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Finans.Fields.OkUrl");
            this.DeletePluginLocaleResource("Plugins.Payments.Finans.Fields.OkUrl.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Finans.Fields.FailUrl");
            this.DeletePluginLocaleResource("Plugins.Payments.Finans.Fields.FailUrl.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Finans.Fields.ApiFinans");
            this.DeletePluginLocaleResource("Plugins.Payments.Finans.Fields.ApiFinans.Hint");

            base.Uninstall();
        }

        #endregion
        #region Properties

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.Manual;
            }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Standard;
            }
        }

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo
        {
            get
            {
                return false;
            }
        }

        #endregion
    }

}
