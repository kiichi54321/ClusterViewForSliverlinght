using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;

namespace WebApplication1
{
    public partial class Startup {

        // 認証の構成の詳細については、http://go.microsoft.com/fwlink/?LinkId=301883 を参照してください。
        public void ConfigureAuth(IAppBuilder app)
        {
            // サインインしたユーザーの情報を保存するために、アプリケーションが Cookie を使用できるようにします
            // また、サード パーティ ログイン プロバイダーを使用してログインしたユーザーに関する情報を保存します。
            // アプリケーションがユーザーのログインを許可している場合、これは必須です
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login")
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // 次の行のコメントを解除して、サード パーティ ログイン プロバイダーを使用したログインを有効にします
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");

            //app.UseFacebookAuthentication(
            //   appId: "",
            //   appSecret: "");

            //app.UseGoogleAuthentication();
        }
    }
}
