using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;

namespace TickerManageProgram
{
    internal class KakaoTalkLogPlatform : ILogPlatform
    {
        public bool initialized { get; private set; }
        string authorizeUrl => $"https://kauth.kakao.com/oauth/authorize?response_type=code&client_id={restApiKey}&redirect_uri={redirectUri}&scope=talk_message";
        string kakaoSendUrl = "https://kapi.kakao.com/v2/api/talk/memo/default/send";


        string restApiKey = "d38dc3c5a68865f07f9efed5fcce0961";
        string redirectUri = "http://localhost:5000/oauth";

        string accessToken = string.Empty;
        string refreshToken = string.Empty;

        // 리프레쉬 토큰을 이용해도 액세스 토큰이 갱신되지 않는 이상현상 대비
        bool lockProcess;



        public async Task<ILogPlatform> Initialize()
        {
            if (initialized)
            { return this; }
            try
            {
                // 브라우저로 인증창 열기
                Process.Start(new ProcessStartInfo
                {
                    FileName = authorizeUrl,
                    UseShellExecute = true
                });

                // 리다이렉트 대기 후 인가 코드 받기
                var listener = new HttpListener();
                listener.Prefixes.Add(redirectUri + "/");
                listener.Start();

                LogChannel.EnqueueLog(new Log(Log.LogType.system, "Redirect 대기중... 브라우저에서 로그인 진행하세요."));

                var context = await listener.GetContextAsync();
                var request = context.Request;
                string authCode = request.QueryString["code"] ?? string.Empty;
                listener.Stop();
                LogChannel.EnqueueLog(new Log(Log.LogType.system, "리다이렉트 감지.."));

                // 액세스 토큰 요청
                var values = new Dictionary<string, string>
                {
                    { "grant_type", "authorization_code" },
                    { "client_id", restApiKey },
                    { "redirect_uri", redirectUri },
                    { "code", authCode }
                };

                using var content = new FormUrlEncodedContent(values);

                using var client = new HttpClient();
                using var response = await client.PostAsync("https://kauth.kakao.com/oauth/token", content);
                string responseBody = await response.Content.ReadAsStringAsync();

                var responseJson = JsonSerializer.Deserialize<JsonElement>(responseBody);

                accessToken = responseJson.GetProperty("access_token").GetString() ?? string.Empty;
                refreshToken = responseJson.GetProperty("refresh_token").GetString() ?? string.Empty;
                initialized = true;
            }
            catch (Exception ex)
            {
                LogChannel.EnqueueLog(new Log(Log.LogType.system, "KakaoTalk Logging Initialization Failed: " + ex.Message));
                return this;
            }
            return this;
        }
        public async Task SendLog(Log log)
        {
            if (lockProcess)
            { return; }
            if (log.type is Log.LogType.ticker or Log.LogType.fj or Log.LogType.test)
            {
                var messageJson = new
                {
                    object_type = "text",
                    text = log.message,
                    link = new { web_url = "https://developers.kakao.com" },
                    button_title = "바로가기"
                };
                string jsonBody = JsonSerializer.Serialize(messageJson);

                using StringContent messageContent = new StringContent($"template_object={jsonBody}", Encoding.UTF8, "application/x-www-form-urlencoded");

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                using var sendResponse = await client.PostAsync(kakaoSendUrl, messageContent);
                string sendResult = await sendResponse.Content.ReadAsStringAsync();

                if (sendResponse.IsSuccessStatusCode)
                {
                }
                else if (sendResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    LogChannel.EnqueueLog(new Log(Log.LogType.system, "액세스 토큰 만료. 갱신 시도."));
                    await RefreshAccessToken();
                    await SendLog(log);
                }
                else
                {
                }
                return;
            }
        }

        async Task RefreshAccessToken()
        {
            if (lockProcess)
            { return; }

            var refreshValues = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "client_id", restApiKey },
                { "refresh_token", refreshToken }
            };

            using var client = new HttpClient();
            using var refreshContent = new FormUrlEncodedContent(refreshValues);
            using var refreshResponse = await client.PostAsync("https://kauth.kakao.com/oauth/token", refreshContent);

            if (!refreshResponse.IsSuccessStatusCode)
            {
                LogChannel.EnqueueLog(new Log(Log.LogType.system, "액세스 토큰 갱신 실패: " + refreshResponse.StatusCode));
                lockProcess = true;
                return;
            }

            string refreshBody = await refreshResponse.Content.ReadAsStringAsync();
            var refreshData = JsonSerializer.Deserialize<JsonElement>(refreshBody);

            accessToken = refreshData.GetProperty("access_token").GetString() ?? string.Empty;
        }
    }
}
