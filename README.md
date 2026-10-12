# MailImporter

MailImporter is a Windows service that imports incoming e-mail messages into TaskBeat. Depending on the message subject, an e-mail either creates a new task or adds a comment to an existing task.

## What the service does

At runtime, the service:

1. Loads the list of watched mailboxes from `Users.xml` located next to the executable. The file is reloaded only when its content changes (detected by a SHA1 hash), which lets you add or remove accounts without restarting the service.
2. On a timer (configurable, default **25 seconds**) processes every configured mailbox in parallel on worker threads:
    - Connects over **IMAP** (with or without SSL) and selects the `INBOX`.
    - Searches for **unread (`UNSEEN`)** messages.
    - Parses each message and, when it matches the rules below, creates a task or a comment in TaskBeat.
    - Deletes messages that were processed successfully; unmatched or failed messages stay unread on the server.
3. Skips a polling cycle when the previous one is still running, and cancels mailbox jobs that exceed the global timeout (default **30 seconds**).

## How messages are mapped to tasks

The subject line decides what happens with a message:

| Subject | Result |
|---------|--------|
| Contains `#new` | Creates a **new task**. The `#new` tag is stripped, and the rest of the subject becomes the task title. The message body becomes the task notes/comment. |
| Ends with `#<number>` (e.g. `Re: update #123`) | Adds a **comment** to the task with reference number `123`. The message body becomes the comment text. |
| Anything else | Ignored (message stays unread). |

Additional rules:

- The sender's e-mail address must match an existing TaskBeat profile; otherwise the message is skipped and logged.
- Everything after a line containing only `--` in the message body is discarded, so signatures and quoted replies do not end up in task content.

## Configuration (`App.config`)

| Key | Default | Description |
|-----|---------|-------------|
| `MailServerCheckerInterval` | `25000` | Interval in milliseconds between mail server checks. |
| `MailServerPort` | `143` | Default IMAP port, used when `port` is missing or invalid in `Users.xml`. |
| `UseSsl` | `false` | Default SSL setting, used when `ssl` is missing or invalid in `Users.xml`. |
| `MsSQL_Database` | – | TaskBeat database name. |
| `MsSQL_Username` | – | Database user. |
| `MsSQL_Password` | – | Database password. |
| `MsSQL_Hostname` | – | SQL Server instance (e.g. `.\SQLEXPRESS`). |
| `DataCacheFolder` | – | Folder used for temporary data cache files. |

## Mailboxes (`Users.xml`)

Each `<user>` element describes one mailbox. Attributes `username`, `password` and `host` are required; `port` and `ssl` fall back to the defaults from `App.config`.

```xml
<root>
	<user username="notification@example.com" password="your-password" host="imap.example.com" port="143" ssl="false"/>
</root>
```

> **Note:** passwords are stored in plain text. Make sure the folder with `Users.xml` is readable only by the service account.

## Logging

Logging is configured in `Log4Net.config` with two appenders:

- `log.txt` in the service folder — rolling file, 10 MB per file, 5 backups kept.
- Windows Event Log, source `MailImporter`.

## Build and install

The project targets **.NET Framework 4.5.3 (x86)** and is built as part of the TaskBeat solution (`TaskBeat.Bootstrapper` and `TaskBeat.Logic` are project references). It uses **ActiveUp.Net** for IMAP, **StructureMap** for dependency injection and **log4net** for logging.

1. Build the solution in Visual Studio or with MSBuild (Release|x86).
2. Install the executable as a Windows service:

   ```powershell
   C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe MailImporter.exe
   ```

   The `ProjectInstaller` registers the service as `MailImporter` running under **LocalSystem** with automatic startup.

3. Start it from `services.msc` or with:

   ```powershell
   net start MailImporter
   ```

To uninstall, run `InstallUtil.exe /u MailImporter.exe`.
