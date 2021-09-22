using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.Finans.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Finans.Fields.MbrId")]
        public string MbrId { get; set; }
        public bool MbrId_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Payments.Finans.Fields.MerchantID")]
        public string MerchantID { get; set; }
        public bool MerchantID_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Payments.Finans.Fields.MerchantPass")]

        public string MerchantPass { get; set; }
        public bool MerchantPass_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Finans.Fields.UserCode")]
        public string UserCode { get; set; }
        public bool UserCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Finans.Fields.OkUrl")]
        public string OkUrl { get; set; }
        public bool OkUrl_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Payments.Finans.Fields.FailUrl")]
        public string FailUrl { get; set; }
        public bool FailUrl_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Payments.Finans.Fields.ApiFinans")]
        public string ApiFinans { get; set; }
        public bool ApiFinans_OverrideForStore { get; set; }



        public int TransactModeId { get; set; }
        public bool TransactModeId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Finans.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Manual.Finans.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }


    }
}
