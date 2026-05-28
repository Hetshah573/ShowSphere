namespace ShowSphere.Domain.Enums;

public enum BookingStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Expired = 3,
    Failed = 4
}

public enum PaymentStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
    Refunded = 3
}

public enum PaymentMethod
{
    CreditCard = 0,
    DebitCard = 1,
    UPI = 2,
    NetBanking = 3,
    Wallet = 4
}

public enum SeatCategory
{
    Silver = 0,
    Gold = 1,
    Platinum = 2,
    Recliner = 3
}

public enum ScreenType
{
    Standard = 0,
    IMAX = 1,
    Dolby = 2,
    FourDX = 3
}

public enum MovieCertificate
{
    U = 0,
    UA = 1,
    A = 2,
    S = 3
}
