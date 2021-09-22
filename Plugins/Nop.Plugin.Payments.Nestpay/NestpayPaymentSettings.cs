using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.Nestpay
{
  public  class NestpayPaymentSettings:ISettings
    {
        public string UserName { get; set;  }
       public string Parola { get; set; }
        public string ClientID { get; set; }
        public string Domain  { get; set; }
        public string Domain3D  { get; set; }
       // public string MagazaID { get; set; } 
      //  public string  TerminalID { get; set; }
        public string Key  { get; set; } 
      
        public TransactMode TransactMode { get; set; }
        public bool UseSandbox { get; set; }
     
      //  public string TransactionKey { get; set; }
        //public string LoginId { get; set; }
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

