namespace ApiAutomation.App.Models
{
    public abstract class BaseTransactionRequest<T> where T : class
    {
        public T Charge { get; set; }

        protected BaseTransactionRequest(T chargeData)
        {
            Charge = chargeData;
        }
    }
}