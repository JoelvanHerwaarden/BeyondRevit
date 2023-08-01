using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace BeyondRevit.Apollo
{
    internal class ChatGPTEndPoints
    {
        public static string Key = "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";
        public static string BaseUrl = @"https://api.openai.com/v1/chat/completions";

        public static string SendMessage(string textmessage, string key = "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", string chatGPTModel = "gpt-3.5-turbo")
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("authorization", String.Format("Bearer {0}", key));
            Dictionary<string, dynamic> contentDict = new Dictionary<string, dynamic>()
            {
                {"model", chatGPTModel },
                {"messages", new List<Dictionary<string, string>>() }
            };
            Dictionary<string, string> prompt = new Dictionary<string, string>()
            {
                {"role","user" },
                {"content",textmessage }
            };
            contentDict["messages"].Add(prompt);
            var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(contentDict), System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync(BaseUrl, content).Result;
            string responseText = response.Content.ReadAsStringAsync().Result;
            Utils.Show(responseText);
            var responseDict = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseText);
            string chatGPTResponse = responseDict["choices"][0]["message"]["content"];
            return chatGPTResponse;
        }

        
        

    }


}
