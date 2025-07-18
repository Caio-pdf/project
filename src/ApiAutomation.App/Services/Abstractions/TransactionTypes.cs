namespace ApiAutomation.App.Services
{
    public enum TransactionType
    {
        Undefined,
        EcommerceAuthorization,
        SmartCheckoutAuthorization,
        EcommercePreAuthorization,
        SmartCheckoutPreAuthorization,
        OfflineAuthorization,
        PaymentLinkAuthorization
    }
}