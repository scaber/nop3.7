using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.Nestpay.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Nestpay.Fields.UserName")]
        public string UserName { get; set; }
        public bool UserName_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Payments.Nestpay.Fields.Parola")]
        public string Parola { get; set; }
        public bool Parola_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Payments.Nestpay.Fields.ClientID")]
        
        public string ClientID { get; set; }
        public bool ClientID_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Nestpay.Fields.Domain")]
        public string Domain { get; set; }
        public bool Domain_OverrideForStore { get; set; } 
         

    


        [NopResourceDisplayName("Plugins.Payments.Nestpay.Fields.Domain3D")]
        public string Domain3D { get; set; }
        public bool Domain3D_OverrideForStore   { get; set; }
         
   

        [NopResourceDisplayName("Plugins.Payments.Nestpay.Fields.Key")]
        public string Key { get; set; }
        public bool Key_OverrideForStore { get; set; }
      










        [NopResourceDisplayName("Plugins.Payments.Nestpay.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        public int TransactModeId { get; set; }
        public bool TransactModeId_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Payments.Nestpay.Fields.TransactModeValues")]
        public SelectList TransactModeValues { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Nestpay.Fields.TransactionKey")]
        public string TransactionKey { get; set; }
        public bool TransactionKey_OverrideForStore { get; set; }

        //[NopResourceDisplayName("Plugins.Payments.Nestpay.Fields.LoginId")]
        //public string LoginId { get; set; }
        //public bool LoginId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Nestpay.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Nestpay.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }
    }
}
