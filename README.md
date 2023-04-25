# GmailArchivingToWebDavHelper
Tools for automate archiving email with attachment file (like e-statement) to NAS via WebDAV Protocol

## About this project
I would like to archiving the e-statement (like credit card statement, home load reciept, pay slip) from my email inbox because my email inbox almost full. My solution is get recent email from my inbox and check email header is match by configuration and move the attachment to my NAS use WebDAV protocol

## Configuration
You can see the example configuration file in `GmailArchivingToWebDavHelper/appsettings.json`

### WebDAV setting
```
"WebDev": {
    "BasePath": "", // Base for your WebDAV server path
    "Username": "", // Username for authenticate to WebDAV server
    "Password": "" // Password for authenticate to WebDAV server
  },
```
---

### Mail setting
```
"MailServer": {
    "Host": "", // Mail server host (POP3)
    "Port": "", // Mail server port
    "Username": "", // Username for authenticate mail server
    "Password": "" // Password for authenticate mail server
  },
```
FYI, You mail server is require SSL

---

### Mail filter setting
```
"FilterSettings": [
    {
      "HeaderRegEx": "(?:ttb credit card e-statement)", // Regular Expression for filter email header
      "FilePath": "/Statements/TTB%20Credit%20Card", // Target path WebDAV for save attachment
      "EmailFrom": "estatement@eservice.ttbbank.com", // Email from for filter email sender
      "BodyRegEx": "", // Regular Expression for filter mail message body
      "FileFormatFilter": "" // Regular Expression for filter file format
    }
  ],
  ```
---
### Native cronjob with Quartz.NET
```
"EnableQuartz": "True", // Eanble native cronjob (Quartz.NET) True is enable, False is disable (run only 1 time and exit)
"QuartzTime": "0 0 1 * * ?", // When your enable native cronjob, you can set trigger time
```

---

### Message Driver (For Bot notify)
```
"MessageDriver":  "Telegram", // Select messaging driver
  "Telegram": {
    "ApiKey": "", // Telegram bot API Key
    "ChatId": "" // Telegram bot chat id
  }
```
