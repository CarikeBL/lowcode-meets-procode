# When Low-Code Meets Pro-Code 🚀

This demo shows how a small .NET 10 minimal API can extend the Power Platform by providing server-side logic and actions that low-code makers can call from Power Automate flows.

Why this matters
- **Pro-code** (.NET) hosts business logic, integrations, or advanced AI calls.
- **Low-code** (Power Platform) composes flows, UIs, and automations that call your API as a Custom Connector.
Combining them gives flexibility and scale with minimal friction. 🎯

What’s in this repo

```
lowcode-meets-procode/
 ├─ src/
 │   └─ MakerDotNetApi/        # .NET 10 minimal API
 │        ├─ Program.cs
 │        └─ MakerDotNetApi.csproj
 ├─ README.md                  # You are reading this
 └─ .gitignore
```

Quick setup (PowerShell)

1. Restore and run the API locally:

```powershell
cd C:\Users\User\lowcode-meets-procode\lowcode-meets-procode
dotnet restore
dotnet run --project src\MakerDotNetApi\MakerDotNetApi.csproj
```

2. (Optional) Expose the local port to the internet for Power Automate to call (use ngrok):

```powershell
ngrok http 5000
```

Note: Replace any `http://localhost:5000` references with the `https://` ngrok URL when importing the connector.

Endpoints provided by the demo
- `GET /hello` — returns `{ "message": "Hello from .NET 👋" }`
- `POST /summarize` — accepts `{ "text": "..." }` and returns `{ "summary": "..." }`

Create a Power Automate Custom Connector (step-by-step)

1. Start your API (see Quick setup). If testing remotely, start `ngrok http 5000` and copy the HTTPS URL.
2. In the Power Automate web portal, go to **Data → Custom connectors** and click **+ New custom connector → Import an OpenAPI file**.
3. Upload the OpenAPI JSON from this repo: `src\MakerDotNetApi\MakerDotNetApi-openapi.json`.
4. Edit the **Host** / **Base URL** to match your running service:
	- For local testing with ngrok, set the host to the ngrok hostname (e.g. `abcd-1234.ngrok.io`) and scheme to `https`.
	- Or set the **Host** to `localhost:5000` when using a local gateway.
5. Security: If you use the example `ApiKeyAuth` (header `x-api-key`), set the connector security to **API key** and the header name to `x-api-key`. For production, prefer OAuth2/Azure AD.
6. Click **Create connector**. In the connector editor go to **Definition** to review the imported operations (`GET /hello`, `POST /summarize`).
7. Go to **Test** inside the connector, create a connection (supply the api key if required), and run a quick test for each operation.

Using the connector in a flow (example)

Flow idea: Manually trigger a flow, summarize text, and post the result to Microsoft Teams.

Step-by-step flow description:

1. Trigger: **Manually trigger a flow** (or any other trigger — e.g., When a new file is created).
2. Action: **YourCustomConnector → PostSummarize**
	- Input: the `text` value (could be a Compose output, file contents, or a dynamic field).
3. Action: **Microsoft Teams → Post a message (V3)**
	- Team: choose your Team
	- Channel: choose your Channel
	- Message: `Summary: @{body('PostSummarize')?['summary']}`

Notes and tips
- Keep your responses small and predictable — flows work best with compact outputs.
- Secure the API for production: use OAuth2/Azure AD, or at minimum an API key passed in `x-api-key` header.
- If your API uses Swagger/OpenAPI, Power Automate can import it directly (we included `MakerDotNetApi-openapi.json`).

Examples (PowerShell)

GET hello:
```powershell
Invoke-RestMethod -Method Get -Uri http://localhost:5000/hello
```

POST summarize:
```powershell
$body = @{ text = "This is a long piece of text that should be summarized." } | ConvertTo-Json
Invoke-RestMethod -Method Post -Uri http://localhost:5000/summarize -Body $body -ContentType "application/json"
```

Want help next? 🤝
- I can add API key validation to `Program.cs` and update the OpenAPI file.
- I can generate a connector package (icon + settings) ready to import.
- I can create a sample Power Automate flow JSON you can import.

Enjoy building — When low-code meets pro-code, amazing automations happen! ✨