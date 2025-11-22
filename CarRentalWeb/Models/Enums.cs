namespace VehicleRentalWeb.Models
{
    public enum RegistrationType
    {
        Online,
        WalkIn,
        Agent
    }

    public enum RentalStatus
    {
        Pending,
        Approved,
        Rejected,
        Cancelled
    }

    public enum PaymentStatus
    {
        Pending,
        Paid,
        Failed
    }
}
