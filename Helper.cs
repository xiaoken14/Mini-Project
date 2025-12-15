using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;

namespace Mini_Project;

public class Helper(IHttpContextAccessor ct, IConfiguration cf)
{
    // ------------------------------------------------------------------------
    // Security Helper Functions
    // ------------------------------------------------------------------------
    
    private readonly PasswordHasher<object> ph = new();

    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return "";
        }
        return ph.HashPassword(0, password);
    }

    public bool VerifyPassword(string hash, string password)
    {
        return ph.VerifyHashedPassword(0, hash, password)
               == PasswordVerificationResult.Success;
    }

    public async Task SignIn(string email, string role, bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, email),
            new(ClaimTypes.Role, role),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var properties = new AuthenticationProperties
        {
            IsPersistent = rememberMe
        };

        await ct.HttpContext!.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            properties
        );
    }

    public async Task SignOut()
    {
        await ct.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public string RandomPassword(int length = 10)
    {
        const string s = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var password = "";
        var r = new Random();

        for (int i = 0; i < length; i++)
        {
            password += s[r.Next(s.Length)];
        }

        return password;
    }

    // ------------------------------------------------------------------------
    // Email Helper Functions
    // ------------------------------------------------------------------------

    public void SendEmail(MailMessage mail)
    {
        string user = cf["Smtp:User"] ?? "";
        string pass = cf["Smtp:Pass"] ?? "";
        string name = cf["Smtp:Name"] ?? "";
        string host = cf["Smtp:Host"] ?? "";
        int port = cf.GetValue<int>("Smtp:Port");

        mail.From = new MailAddress(user, name);

        using var smtp = new SmtpClient
        {
            Host = host,
            Port = port,
            EnableSsl = true,
            Credentials = new NetworkCredential(user, pass),
        };

        smtp.Send(mail);
    }

    // ------------------------------------------------------------------------
    // DateTime Helper Functions
    // ------------------------------------------------------------------------

    // Return January (1) to December (12)
    public SelectList GetMonthList()
    {
        var list = new List<object>();

        for (int n = 1; n <= 12; n++)
        {
            list.Add(new
            {
                Id = n,
                Name = new DateTime(1, n, 1).ToString("MMMM"),
            });
        }

        return new SelectList(list, "Id", "Name");
    }

    // Return min to max years
    public SelectList GetYearList(int min, int max, bool reverse = false)
    {
        var list = new List<int>();

        for (int n = min; n <= max; n++)
        {
            list.Add(n);
        }

        if (reverse) list.Reverse();

        return new SelectList(list);
    }

    // ------------------------------------------------------------------------
    // Shopping Cart Helper Functions
    // ------------------------------------------------------------------------

    public Dictionary<string, int> GetCart()
    {
        return ct.HttpContext!.Session.Get<Dictionary<string, int>>("Cart") ?? [];
    }

    public void SetCart(Dictionary<string, int>? dict = null)
    {
        if (dict == null)
        {
            ct.HttpContext!.Session.Remove("Cart");
        }
        else
        {
            ct.HttpContext!.Session.Set("Cart", dict);
        }
    }
}
