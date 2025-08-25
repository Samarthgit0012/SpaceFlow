using System;
using System.Security.Cryptography;
using System.Text;

namespace SpaceFlow.Utilities
{
    /// <summary>
    /// Utility class for generating Razorpay-compatible HMAC signatures for testing
    /// </summary>
    public static class RazorpaySignatureGenerator
    {
        /// <summary>
        /// Generates a HMAC-SHA256 signature for testing payment verification
        /// </summary>
        /// <param name="orderId">The razorpay_order_id</param>
        /// <param name="paymentId">The razorpay_payment_id</param>
        /// <param name="keySecret">Your Razorpay key secret</param>
        /// <returns>The HMAC signature that Razorpay would generate</returns>
        public static string GenerateSignature(string orderId, string paymentId, string keySecret)
        {
            string payload = $"{orderId}|{paymentId}";
            return GenerateHmacSha256(payload, keySecret);
        }

        /// <summary>
        /// Generate HMAC-SHA256 hash
        /// </summary>
        private static string GenerateHmacSha256(string payload, string key)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);
            
            using (var hmac = new HMACSHA256(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(payloadBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        /// <summary>
        /// Example usage for testing
        /// </summary>
        public static void ExampleUsage()
        {
            string orderId = "order_test123";
            string paymentId = "pay_test456"; 
            string keySecret = "yVDDF9cO2QWVdZ2DCqSIIbZq";

            string signature = GenerateSignature(orderId, paymentId, keySecret);
            
            Console.WriteLine($"Order ID: {orderId}");
            Console.WriteLine($"Payment ID: {paymentId}");
            Console.WriteLine($"Generated Signature: {signature}");
            Console.WriteLine();
            Console.WriteLine("Use this in your test JSON:");
            Console.WriteLine($@"{{
  ""razorpay_order_id"": ""{orderId}"",
  ""razorpay_payment_id"": ""{paymentId}"",
  ""razorpay_signature"": ""{signature}"",
  ""bookingId"": ""1""
}}");
        }
    }
}