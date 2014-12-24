﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoubanFM.Universal.API.Models
{
    [JsonObject]
    public class LoginResult
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("err")]
        public string Err { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("expire")]
        public string Expire { get; set; }

        [JsonProperty("r")]
        public string R { get; set; }

        [JsonProperty("user_name")]
        public string UserName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

    }
}
