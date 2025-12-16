# OTP Email Verification - Development Guide

## How to Get OTP Codes for Testing

Since you're running in development mode, the app won't send real emails. Instead, you can get the OTP codes in several ways:

### Method 1: Check the Console Output
When you register a user, look at the console where you ran `dotnet run`. You'll see log messages like:
```
warn: HealthcareApp.Services.DevelopmentEmailService[0]
      === DEVELOPMENT MODE ===
warn: HealthcareApp.Services.DevelopmentEmailService[0]
      OTP for user@example.com: 123456
warn: HealthcareApp.Services.DevelopmentEmailService[0]
      ======================
```

### Method 2: Check the OTP File
The app automatically saves OTP codes to a file called `otp_codes.txt` in your project directory. Open this file to see all generated OTP codes with timestamps.

### Method 3: Use the Development Helper (Recommended)
1. Register a new user
2. On the OTP verification page, you'll see a yellow "Development Mode" box
3. Click the "Get OTP for Testing" button
4. The OTP will appear and be automatically filled in the input field

### Method 4: Direct API Call
You can also call the development endpoint directly:
```
GET http://127.0.0.1:5003/Account/GetOTP?email=user@example.com
```

## To Enable Real Email Sending

If you want to send real emails, update the `appsettings.json` file with your actual SMTP settings:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "Port": "587",
    "Username": "your-actual-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "your-actual-email@gmail.com"
  }
}
```

For Gmail, you'll need to:
1. Enable 2-factor authentication
2. Generate an "App Password" (not your regular password)
3. Use the app password in the configuration

## Testing the OTP Flow

1. Run the app: `dotnet run`
2. Navigate to: http://127.0.0.1:5003
3. Register a new user
4. Use any of the methods above to get the OTP
5. Enter the OTP on the verification page
6. Your account will be confirmed and you can log in

The OTP expires in 10 minutes and you have a maximum of 5 attempts before needing to request a new one.