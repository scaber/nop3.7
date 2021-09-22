using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.Finans
{
  public  class FinansPaymentSettings:ISettings
    {
        public TransactMode TransactMode { get; set; } 
        public string MbrId { get; set;  }
        public string MerchantID { get; set; }
        public string MerchantPass { get; set; }
        public string UserCode { get; set; } 
        public string OkUrl { get; set; }
        public string FailUrl { get; set; } 
        public string ApiFinans { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
        /// </summary>
        public bool AdditionalFeePercentage { get; set; }
        /// <summary>
        /// Additional fee
        /// </summary>
        public decimal AdditionalFee { get; set; }

    }
}

