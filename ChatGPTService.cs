using System;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;


    namespace ChatGPTAppSqlServerAdvWithQuery
    {
        public class ChatGPTService
        {
            private readonly string _apiKey;
            private readonly string _connectionString;

            private bool _isOnline;

        public ChatGPTService(string apiKey, string connectionString)
            {
                _apiKey = apiKey;
                _connectionString = connectionString;
                _isOnline = true; // الوضع الافتراضي هو متصل
             }

        public void SetMode(bool isOnline)
        {
            _isOnline = isOnline;
        }

        public async Task<string> SendMessageAsync(string message)
            {
                //try
                //{
                //    // حاول الحصول على الرد عبر الإنترنت
                //    string response = await GetChatGPTResponseOnline(message);
                //    return response;
                //}
                //catch (Exception)
                //{
                //    // إذا فشل الاتصال عبر الإنترنت، استخدم الرد المخزن محليًا
                //    return GetChatGPTResponseOffline(message);
                //}

                if (_isOnline)
                {
                    return await GetChatGPTResponseOnline(message);
                }
                else
                {
                    return GetChatGPTResponseOffline(message);
            }
        }

            private async Task<string> GetChatGPTResponseOnline(string message)
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

                    var requestContent = new JObject
                {
                    { "model", "gpt-4" },
                    { "messages", new JArray(
                        new JObject
                        {
                            { "role", "user" },
                            { "content", message }
                        })
                    }
                };

                    var content = new StringContent(requestContent.ToString(), System.Text.Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseString = await response.Content.ReadAsStringAsync();
                        var jsonResponse = JObject.Parse(responseString);
                        return jsonResponse["choices"][0]["message"]["content"].ToString();
                    }
                    else
                    {
                        throw new Exception($"API request failed with status code {response.StatusCode}: {response.ReasonPhrase}");
                    }
                }
            }



            private string GetChatGPTResponseOffline(string message)
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    SqlCommand command = new SqlCommand("SearchResponses", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Prompt", message);

                    connection.Open();
                    var result = command.ExecuteScalar();
                    if (result != null)
                    {
                        return result.ToString();
                    }
                    else
                    {
                        return "لا توجد استجابة محفوظة مسبقًا لهذا الاستعلام.";
                    }
                }
        }


    }
}
