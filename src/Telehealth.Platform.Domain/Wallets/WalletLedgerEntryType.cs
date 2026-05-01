namespace Telehealth.Platform.Domain.Wallets;

public enum WalletLedgerEntryType
{
    CreditPurchased = 1,
    CreditReserved = 2,
    CreditReservationReleased = 3,
    ConsultationCharged = 4,
    RefundGranted = 5,
    PromotionalCreditGranted = 6,
    AdminAdjustment = 7,
    Expiry = 8,
}
