# HN-27: one-to-one chat

The existing `Conversation` and `ChatMessage` models and
`AddConversationsAndChatMessages` migration store chat in SQL Server. The API
applies migrations automatically in Development.

## API

All chat requests require a Keycloak access token for `hirenow-api`.

| Request | Behavior |
| --- | --- |
| `POST /conversations` with `{ "sellerId": "<Keycloak sub>" }` | Buyers start or reopen a conversation with another user who has a profile or published service. Returns its `id`, `buyerId`, and `sellerId`. |
| `GET /conversations` | Lists only the current user's conversations, including the other participant's display name. |
| `GET /conversations/{id}/messages` | Returns history in chronological order. Returns 404 when the conversation does not exist or the caller is not a participant. |

The authorized SignalR hub is at `/hubs/chat`:

- Invoke `SendMessage(conversationId, content)` to send text. Content is trimmed
  and must contain 1–2000 characters.
- Listen for `ReceiveMessage`. The payload is
  `{ id, conversationId, senderId, content, sentAt }`, with a UTC timestamp.
- `SendMessage` also returns that payload. The frontend deduplicates by message ID.
- The service checks participation and saves the message before the hub sends it
  to both participants' connections. The sender ID comes from authentication.

`MapInboundClaims` stays disabled. `KeycloakUserIdProvider` explicitly uses `sub`
as SignalR's user identifier, matching REST and the stored participant IDs.
The JWT handler accepts `access_token` query parameters only under `/hubs/chat`,
which supports browser WebSocket/SSE authentication. The client refreshes its
Keycloak token before connecting, reconnects automatically, and reloads history
after reconnection. CORS permits credentials from the configured frontend origins.
See [Microsoft's SignalR authentication documentation](https://learn.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz?view=aspnetcore-10.0).

## Try it locally

1. Run `docker compose up --build -V` to install the SignalR client in the
   frontend container and start the updated API.
2. Sign in as `seller` and publish a service under **My services**, or save your
   profile. The authenticated `GET /me` response contains your Keycloak user ID.
3. In a separate browser session (for example, a private window), sign in as
   `buyer`. Open the seller's service page at `/services/<service-id>` or profile
   at `/sellers/<seller-user-id>` and choose **Contact seller**. This creates or
   reopens the conversation and opens `/chat/<conversation-id>`. No message is
   sent until the buyer types one and chooses **Send**.
4. As the seller, open **Chat** in the navigation. Send a message as the buyer;
   the conversation appears for the seller, who can select it and reply.
5. Send messages in both directions. Refresh either window to confirm history
   is retained. Choosing **Contact seller** again reopens the same conversation,
   including when contacting the seller from a different service.

Use the development user password configured in `.env`. No extra authentication
setup is required. A seller with a published service can receive messages even
without a saved profile. The button is shown only to buyers contacting another
user; the backend also requires the Buyer role when starting a conversation.

If your Keycloak volume predates the fixed user IDs in the realm seed, the
sample listings may belong to a different ID than the account you sign in with.
For a two-account demo, use the seller ID returned by `GET /me` and that account's
own profile or published services. The sample-only seller Rita has no login account.

## Verification

Backend and frontend builds pass, and EF Core reports no pending model changes.
A manual smoke check using the real local Keycloak, SQL Server, and two SignalR
WebSocket clients verified bidirectional delivery, persisted history, reuse of
an existing conversation, query-string JWT authentication, and rejection of
anonymous users, nonparticipants, self-chat, missing profiles, and invalid text.
No unit tests were added.
