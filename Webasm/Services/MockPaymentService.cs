namespace UserApp.Services
{
    public class MockPaymentService
    {
        public bool ProcessPayment(decimal amount)
        {
            // Simulate a successful payment 90% of the time, and a failure 10%
            Random random = new Random();
            return random.Next(1, 101) <= 90;
        }
    }
}