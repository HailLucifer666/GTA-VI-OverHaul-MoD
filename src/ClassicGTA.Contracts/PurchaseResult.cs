namespace ClassicGTA.Contracts
{
    public enum PurchaseStatus
    {
        Succeeded = 0,
        AlreadyOwned = 1,
        UnknownAsset = 2,
        InsufficientFunds = 3,
        PersistenceFailedAndRefunded = 4,
        BlockedInOnline = 5,
        InvalidQuantity = 6
    }

    public sealed class PurchaseResult
    {
        public PurchaseStatus Status { get; private set; }
        public string Message { get; private set; }

        public bool Succeeded { get { return Status == PurchaseStatus.Succeeded; } }

        public PurchaseResult(PurchaseStatus status, string message)
        {
            Status = status;
            Message = message;
        }
    }
}
