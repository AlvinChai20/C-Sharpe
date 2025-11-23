namespace Webasm.Service
{
    public class helper
    {
        private bool ValidateCaptcha(string captchaResponse)
        {
            var secretKey = "6Lfa0M4rAAAAAO8aaZeSFZvkujTNv_51fBKzQ7f_";
            using var client = new HttpClient();
            var result = client.PostAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={captchaResponse}",
                null).Result;

            var json = result.Content.ReadAsStringAsync().Result;
            dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            return data.success == "true";
        }

    }
}
