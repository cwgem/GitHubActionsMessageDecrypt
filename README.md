# GitHubActionsMessageDecrypt


## Why?

I wanted to understand what data was behind GitHub Self-Hosted Runners when getting messages from GitHub. Unfortunately, the encryption for it is a bit layered so I looked around for solutions on how to decrypt messages with less of a hassle. It could also be useful for debugging purposes.

## Sources

This is a combaination of code from [MessageListener.cs](https://github.com/actions/runner/blob/main/src/Runner.Listener/MessageListener.cs), [RSAFileKeyManager.cs](https://github.com/actions/runner/blob/main/src/Runner.Listener/Configuration/RSAFileKeyManager.cs), and [IOUtil.cs](https://github.com/actions/runner/blob/main/src/Runner.Sdk/Util/IOUtil.cs#L43) from the [GitHub Actions runner repository](https://github.com/actions/runner). The overall code flow was adjusted and updated from Pulse Security's article [Azure DevOps CICD Pipelines - Command Injection with Parameters, Variables and a discussion on Runner hijacking](https://pulsesecurity.co.nz/advisories/Azure-Devops-Command-Injection) to make it run as a standalone application.

## Requirements

- .NET 7.0 SDK
- Unironically Linux because Windows has some weird file protect mechanism that I didn't want to deal with at the time
- Currently meant to support GitHub public actions, but may work on GitHub Enterprise as well

## Installation

Simply run `dotnet build` in the project directory.

## Preparation

In order for this to work you will need:

- A MITM proxy to capture traffic such as [mitmproxy](https://docs.mitmproxy.org/stable/overview-installation/)
- GitHub actions runner configured and setup to use the MITM proxy via the `http_proxy` environment variable

Run your MITM proxy and then start your GitHub self-hosted runner. When ready, run your job via the GitHub actions.


## Usage

1. Look for a `POST` call that looks similar to `POST https://pipelinesghubeus3.actions.githubusercontent.com/[random_string_here]/_apis/distributedtask/pools/1/sessions` (Make sure it's a 200 return)
2. Find the `encryptionKey` JSON key and then grab the value (best to grab it with quotes included when pasting it in the console)
3. Find a message you want to decode. They look something like this: `GET https://pipelinesghubeus3.actions.githubusercontent.com/[random_string]/_apis/distributedtask/pools/1/messages?sessionId=[session_id]&status=Online&runnerVersion=2.309.0`
4. Grab the body value as is and put it into a temporary file
5. Grab the "iv" value
6. Get the path to your GitHub actions runner installation's `.credentials_rsaparams`

`dotnet run [step 6 value] [step 2 value] [step 5 value] [path to step 4 temporary file] > unencrypted_message.json`

If the Runner was properly first time configured and the proper values were obtained properly you'll have `unencrypted_message.json` with the message unencrypted
