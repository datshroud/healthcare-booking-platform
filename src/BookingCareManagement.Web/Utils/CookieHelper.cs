using System;

namespace BookingCareManagement.Utils;

public class CookieHelper
{
    public static void SetAuthCookies(HttpResponse res, string accessToken,
        DateTime accessExpiresUtc, string refreshToken, DateTime refreshExpiresUtc)
    {
        var secure = true;
        var sameSite = SameSiteMode.Lax;
        res.Cookies.Append("access_token", accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = sameSite,
            Expires = accessExpiresUtc
        });
        res.Cookies.Append("refresh_token", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = sameSite,
            Expires = refreshExpiresUtc
        });
    }

    public static void ClearAuthCookies(HttpResponse res)
    {
        res.Cookies.Delete("access_token");
        res.Cookies.Delete("refresh_token");
    }

}
