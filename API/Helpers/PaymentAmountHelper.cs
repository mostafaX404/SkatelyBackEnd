namespace API.Helpers;

public static class PaymentAmountHelper
{
    public static long ToCents(decimal amount) =>
        (long)Math.Round(amount * 100, MidpointRounding.AwayFromZero);

    public static long CalculateTotalInCents(decimal subtotal, decimal shipping) =>
        ToCents(subtotal + shipping);
}
