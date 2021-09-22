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
using Nop.Plugin.Payments.Nestpay.Controllers;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using ePayment;
using System.Collections;

namespace Nop.Plugin.Payments.Nestpay
{
    public class NestpayPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly NestpayPaymentSettings _nestpayPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly CurrencySettings _currencySettings;
        private readonly IWebHelper _webHelper;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IEncryptionService _encryptionService;

        #endregion

        #region Ctor

        public NestpayPaymentProcessor(NestpayPaymentSettings nestpayPaymentSettings,
            ISettingService settingService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            CurrencySettings currencySettings, IWebHelper webHelper,
            IOrderTotalCalculationService orderTotalCalculationService, IEncryptionService encryptionService)
        {
            this._nestpayPaymentSettings = nestpayPaymentSettings;
            this._settingService = settingService;
            this._currencyService = currencyService;
            this._customerService = customerService;
            this._currencySettings = currencySettings;
            this._webHelper = webHelper;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._encryptionService = encryptionService;
        }

        #endregion
        #region apiler
        private string GetAuthorizeNetUrl()
        {
            return _nestpayPaymentSettings.UseSandbox ?
                " https://entegrasyon.asseco-see.com.tr/fim/api " : "https://entegrasyon.asseco-see.com.tr/fim/api";//eklneceek gerçek


        }
        private string GetApiVersion()
        {
            return "3.1";
        }

        #endregion
        #region Methods




        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        /// 
        public static string RandomNumber()
        {
            Random r = new Random();
            string strRsayi = r.Next(1, 100000).ToString() + String.Format("{0:T}", DateTime.Now).Replace(":", string.Empty);
            return strRsayi;
        }
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
            ePayment.cc5payment mycc5pay = new ePayment.cc5payment();
            var result = new ProcessPaymentResult();

            var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);

            var webClient = new WebClient();
            var form = new NameValueCollection();

            System.Net.HttpWebRequest r = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(_nestpayPaymentSettings.Domain);
            r.Method = "POST";
            r.ContentType = "application/x-www-form-urlencoded";
            System.IO.Stream bidon = r.GetRequestStream(); { bidon.Write(new byte[0], 0, 0); bidon.Close(); }

 

                mycc5pay.host = _nestpayPaymentSettings.Domain;
                mycc5pay.name = _nestpayPaymentSettings.UserName;
                mycc5pay.password = _nestpayPaymentSettings.Parola;
                mycc5pay.clientid = _nestpayPaymentSettings.ClientID;
                mycc5pay.orderresult = 1;
                mycc5pay.cardnumber = processPaymentRequest.CreditCardNumber.ToString();
                mycc5pay.expmonth = string.Format("{0:00}", processPaymentRequest.CreditCardExpireMonth);
                mycc5pay.expyear = processPaymentRequest.CreditCardExpireYear.ToString().Substring(2,2);
                mycc5pay.cv2 = string.Format("{0:000}", processPaymentRequest.CreditCardCvv2 ); 

                mycc5pay.chargetype = "Auth";
                var orderTotal = Math.Round(processPaymentRequest.OrderTotal, 2);
                mycc5pay.subtotal = orderTotal.ToString("0.00", CultureInfo.InvariantCulture);
                mycc5pay.oid = RandomNumber();
                mycc5pay.groupid = "123";

                mycc5pay.currency = "949";//TL birimi eklenecek
                mycc5pay.bname = customer.BillingAddress.FirstName;
                mycc5pay.ip = GetIp();
                





                string bankaSonuc = mycc5pay.processorder();//metot çağrılır              
                string bankaOid = mycc5pay.oid;//dönen order id 
                string bankaAppr = mycc5pay.appr;// dönen provizyon numarası
                string bankaProv = mycc5pay.code;
                string bankaHata = mycc5pay.errmsg;//dönen hata mesajı
                string transId = mycc5pay.transid;


                if (bankaSonuc=="1" & bankaAppr=="Approved")
                {

                    return result;

                    
                }


                else
                {
                    result.AddError(bankaHata);
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
                _nestpayPaymentSettings.AdditionalFee, _nestpayPaymentSettings.AdditionalFeePercentage);
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
            switch (_nestpayPaymentSettings.TransactMode)
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
            controllerName = "PaymentNestpay";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.Nestpay.Controllers" }, { "area", null } };
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
            controllerName = "PaymentNestpay";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.Nestpay.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(PaymentNestpayController);
        }

        public override void Install()
        {
            //settings
            var settings = new NestpayPaymentSettings
            {
                TransactMode = TransactMode.Authorize
            };
            _settingService.SaveSetting(settings);


            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Nestpay.Fields.AdditionalFee", "Additional fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Nestpay.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Nestpay.Fields.AdditionalFeePercentage", "Additional fee. Use percentage");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Nestpay.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage  ");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Nestpay.Fields.TransactMode", "After checkout mark payment as");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Nestpay.Fields.TransactMode.Hint", "Specify transaction mode.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Nestpay.Fields.UserName", "KullanıcıAdı");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Nestpay.Fields.UserName.Hint", "KullanıcıAdı");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Nestpay.Fields.Parola", "Parola");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Nestpay.Fields.Parola.Hint", "Parola.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Nestpay.Fields.ClientID", "ClientID");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Nestpay.Fields.ClientID.Hint", "ClientID");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Nestpay.Fields.Domain", "Domain.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Nestpay.Fields.Domain.Hint", "Domain");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Nestpay.Fields.Domain3D", "Domain3D");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Nestpay.Fields.Domain3D.Hint", "Domain3D");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Nestpay.Fields.Key", "Key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Nestpay.Fields.Key.Hint", "Key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Nestpay.Fields.UseSandbox", "UseSandbox");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Nestpay.Fields.UseSandbox.Hint", "UseSandbox");

            base.Install();
        }

        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<NestpayPaymentSettings>();

            //locales
            this.DeletePluginLocaleResource("Plugins.Payments.Nestpay.Fields.AdditionalFee");
            this.DeletePluginLocaleResource("Plugins.Payments.Nestpay.Fields.AdditionalFee.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Nestpay.Fields.AdditionalFeePercentage");
            this.DeletePluginLocaleResource("Plugins.Payments.Nestpay.Fields.AdditionalFeePercentage.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Nestpay.Fields.TransactMode");
            this.DeletePluginLocaleResource("Plugins.Payments.Nestpay.Fields.TransactMode.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Nestpay.Fields.UserName");
            this.DeletePluginLocaleResource("Plugins.Payments.Nestpay.Fields.UserName.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Nestpay.Fields.Parola");
            this.DeletePluginLocaleResource("Plugins.Payments.Nestpay.Fields.Parola.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Nestpay.Fields.ClientID");
            this.DeletePluginLocaleResource("Plugins.Payments.Nestpay.Fields.ClientID.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Nestpay.Fields.Domain");
            this.DeletePluginLocaleResource("Plugins.Payments.Nestpay.Fields.Domain.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Nestpay.Fields.Domain3D");
            this.DeletePluginLocaleResource("Plugins.Payments.Nestpay.Fields.Domain3D.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Nestpay.Fields.Key");
            this.DeletePluginLocaleResource("Plugins.Payments.Nestpay.Fields.Key.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Nestpay.Fields.UseSandbox");
            this.DeletePluginLocaleResource("Plugins.Payments.Nestpay.Fields.UseSandbox.Hint");

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
