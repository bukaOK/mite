using Yandex.Money.Api.Sdk.Net;

namespace Mite.BLL.ExternalServices.YandexMoney
{
    public class Authenticator : DefaultAuthenticator
    {
        public Authenticator() : base()
        {
        }
        /// <summary>
        /// По умолчанию токеном доступа считается токен текущего(по кукам) пользователя.
        /// Если понадобится переопределить(например для отправки денег из системы) нужно задать токен явно через данный аутентификатор
        /// </summary>
        public override string Token { get; set; }
    }
}